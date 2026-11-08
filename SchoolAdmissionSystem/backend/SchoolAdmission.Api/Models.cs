using Microsoft.EntityFrameworkCore;

namespace SchoolAdmission.Api;

public class AdmissionClass
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
    public List<AdmissionHold> Holds { get; set; } = [];
}
public class AdmissionHold
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClassId { get; set; }
    public AdmissionClass Class { get; set; } = null!;
    public string StudentId { get; set; } = "";
    public string Status { get; set; } = "HELD";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
public class AdmissionDb(DbContextOptions<AdmissionDb> options) : DbContext(options)
{
    public DbSet<AdmissionClass> Classes => Set<AdmissionClass>();
    public DbSet<AdmissionHold> Holds => Set<AdmissionHold>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AdmissionClass>().HasKey(x => x.Id);
        b.Entity<AdmissionClass>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Entity<AdmissionClass>().ToTable(t => t.HasCheckConstraint("CK_Classes_TotalSeats", ""TotalSeats" > 0"));
        b.Entity<AdmissionHold>().HasKey(x => x.Id);
        b.Entity<AdmissionHold>().HasOne(x => x.Class).WithMany(x => x.Holds).HasForeignKey(x => x.ClassId);
        b.Entity<AdmissionHold>().Property(x => x.StudentId).HasMaxLength(17).IsRequired();
        b.Entity<AdmissionHold>().Property(x => x.Status).HasMaxLength(16).IsRequired();
        b.Entity<AdmissionHold>().HasIndex(x => new { x.ClassId, x.Status, x.ExpiresAt });
        b.Entity<AdmissionHold>().HasIndex(x => new { x.StudentId, x.Status });
    }
}
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled API exception");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "Something went wrong. Please try again." }, cancellationToken);
        return true;
    }
}
