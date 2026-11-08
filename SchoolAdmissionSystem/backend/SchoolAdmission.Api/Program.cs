using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AdmissionDb>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AdmissionDb>();
    await db.Database.EnsureCreatedAsync();
    if (!await db.Classes.AnyAsync())
    {
        db.Classes.AddRange(
            new AdmissionClass { Name = "Class 6 - Morning Shift", TotalSeats = 120 },
            new AdmissionClass { Name = "Class 6 - Day Shift", TotalSeats = 80 },
            new AdmissionClass { Name = "Class 7 - Morning Shift", TotalSeats = 100 },
            new AdmissionClass { Name = "Class 8 - Day Shift", TotalSeats = 90 }
        );
        await db.SaveChangesAsync();
    }
}

var holdSeconds = Math.Max(30, int.TryParse(Environment.GetEnvironmentVariable("HOLD_DURATION_SECONDS"), out var seconds) ? seconds : 600);

app.MapGet("/classes", async (AdmissionDb db) =>
{
    var now = DateTime.UtcNow;
    var data = await db.Classes.OrderBy(c => c.Name).Select(c => new ClassDto(
        c.Id, c.Name, c.TotalSeats,
        c.Holds.Count(h => h.Status == "HELD" && h.ExpiresAt > now),
        c.Holds.Count(h => h.Status == "CONFIRMED"),
        c.TotalSeats - c.Holds.Count(h => (h.Status == "HELD" && h.ExpiresAt > now) || h.Status == "CONFIRMED")
    )).ToListAsync();
    return Results.Ok(data);
});

app.MapGet("/classes/{classId:guid}", async (Guid classId, AdmissionDb db) =>
{
    var c = await db.Classes.Include(x => x.Holds).FirstOrDefaultAsync(x => x.Id == classId);
    return c is null ? Results.NotFound(new { message = "Class not found." }) : Results.Ok(ToDto(c));
});

app.MapPost("/classes", async (CreateClassRequest req, AdmissionDb db) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Trim().Length > 120 || req.TotalSeats < 1 || req.TotalSeats > 10000)
        return Results.BadRequest(new { message = "Enter a class name and a seat count between 1 and 10,000." });
    var c = new AdmissionClass { Name = req.Name.Trim(), TotalSeats = req.TotalSeats };
    db.Classes.Add(c);
    await db.SaveChangesAsync();
    return Results.Created($"/classes/{c.Id}", new { id = c.Id, name = c.Name, totalSeats = c.TotalSeats });
});

app.MapPost("/classes/{classId:guid}/holds", async (Guid classId, HttpRequest request, AdmissionDb db) =>
{
    var studentId = request.Headers["X-Student-Id"].ToString();
    if (!ValidStudent(studentId)) return Results.BadRequest(new { message = "Enter a valid 17-digit birth registration number." });

    await using var tx = await db.Database.BeginTransactionAsync();
    // Serialize requests from the same student even when they target different classes.
    await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtext({studentId}))");
    // Lock the capacity row so concurrent requests for the same class are serialized.
    var locked = await db.Classes.FromSqlInterpolated($"SELECT * FROM \"Classes\" WHERE \"Id\" = {classId} FOR UPDATE").FirstOrDefaultAsync();
    if (locked is null) return Results.NotFound(new { message = "Class not found." });

    var now = DateTime.UtcNow;
    var existing = await db.Holds.Where(h => h.StudentId == studentId &&
        ((h.Status == "HELD" && h.ExpiresAt > now) || h.Status == "CONFIRMED")).FirstOrDefaultAsync();
    if (existing is not null) return Results.Conflict(new { message = "You already have an active or confirmed admission application." });

    var activeCount = await db.Holds.CountAsync(h => h.ClassId == classId &&
        ((h.Status == "HELD" && h.ExpiresAt > now) || h.Status == "CONFIRMED"));
    if (activeCount >= locked.TotalSeats) return Results.Conflict(new { message = "No seats are currently available in this class." });

    var hold = new AdmissionHold { ClassId = classId, StudentId = studentId, Status = "HELD", CreatedAt = now, ExpiresAt = now.AddSeconds(holdSeconds) };
    db.Holds.Add(hold);
    await db.SaveChangesAsync();
    await tx.CommitAsync();
    return Results.Created($"/holds/{hold.Id}", new { holdId = hold.Id, classId, studentId, status = hold.Status, expiresAt = hold.ExpiresAt });
});

app.MapGet("/holds/{holdId:guid}", async (Guid holdId, AdmissionDb db) =>
{
    var h = await db.Holds.Include(x => x.Class).FirstOrDefaultAsync(x => x.Id == holdId);
    if (h is null) return Results.NotFound(new { message = "Application not found." });
    var status = h.Status == "HELD" && h.ExpiresAt <= DateTime.UtcNow ? "EXPIRED" : h.Status;
    return Results.Ok(new { holdId = h.Id, classId = h.ClassId, className = h.Class.Name, status, expiresAt = h.ExpiresAt });
});

app.MapPost("/holds/{holdId:guid}/confirm", async (Guid holdId, HttpRequest request, AdmissionDb db) =>
{
    var studentId = request.Headers["X-Student-Id"].ToString();
    if (!ValidStudent(studentId)) return Results.BadRequest(new { message = "A valid X-Student-Id header is required." });
    await using var tx = await db.Database.BeginTransactionAsync();
    var h = await db.Holds.FromSqlInterpolated($"SELECT * FROM \"Holds\" WHERE \"Id\" = {holdId} FOR UPDATE").FirstOrDefaultAsync();
    if (h is null) return Results.NotFound(new { message = "Application not found." });
    if (h.StudentId != studentId) return Results.Forbid();
    if (h.Status == "CONFIRMED") return Results.Ok(new { holdId = h.Id, status = h.Status });
    if (h.Status != "HELD" || h.ExpiresAt <= DateTime.UtcNow)
    {
        if (h.Status == "HELD") { h.Status = "EXPIRED"; await db.SaveChangesAsync(); }
        return Results.Conflict(new { message = "This seat hold has expired. Please apply again." });
    }
    h.Status = "CONFIRMED";
    h.ConfirmedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    await tx.CommitAsync();
    return Results.Ok(new { holdId = h.Id, status = h.Status });
});

app.MapDelete("/holds/{holdId:guid}", async (Guid holdId, HttpRequest request, AdmissionDb db) =>
{
    var studentId = request.Headers["X-Student-Id"].ToString();
    if (!ValidStudent(studentId)) return Results.BadRequest(new { message = "A valid X-Student-Id header is required." });
    var h = await db.Holds.FirstOrDefaultAsync(x => x.Id == holdId);
    if (h is null) return Results.NotFound(new { message = "Application not found." });
    if (h.StudentId != studentId) return Results.Forbid();
    if (h.Status == "CONFIRMED") return Results.Conflict(new { message = "A confirmed admission cannot be cancelled here." });
    if (h.Status == "HELD") { h.Status = h.ExpiresAt <= DateTime.UtcNow ? "EXPIRED" : "CANCELLED"; await db.SaveChangesAsync(); }
    return Results.NoContent();
});

app.MapGet("/me/holds", async (HttpRequest request, AdmissionDb db) =>
{
    var studentId = request.Headers["X-Student-Id"].ToString();
    if (!ValidStudent(studentId)) return Results.BadRequest(new { message = "Enter a valid 17-digit birth registration number." });
    var now = DateTime.UtcNow;
    var items = await db.Holds.Include(h => h.Class).Where(h => h.StudentId == studentId)
        .OrderByDescending(h => h.CreatedAt).Select(h => new {
            holdId = h.Id, classId = h.ClassId, className = h.Class.Name,
            status = h.Status == "HELD" && h.ExpiresAt <= now ? "EXPIRED" : h.Status,
            expiresAt = h.ExpiresAt, createdAt = h.CreatedAt
        }).ToListAsync();
    return Results.Ok(items);
});

app.Run();

static bool ValidStudent(string id) => System.Text.RegularExpressions.Regex.IsMatch(id ?? "", @"^\d{17}$");
static ClassDto ToDto(AdmissionClass c)
{
    var now = DateTime.UtcNow;
    var held = c.Holds.Count(h => h.Status == "HELD" && h.ExpiresAt > now);
    var confirmed = c.Holds.Count(h => h.Status == "CONFIRMED");
    return new(c.Id, c.Name, c.TotalSeats, held, confirmed, Math.Max(0, c.TotalSeats - held - confirmed));
}
public record CreateClassRequest(string Name, int TotalSeats);
public record ClassDto(Guid Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);
