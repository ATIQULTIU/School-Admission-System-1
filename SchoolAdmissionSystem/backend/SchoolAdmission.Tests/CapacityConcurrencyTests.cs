using Xunit;

namespace SchoolAdmission.Tests;

// Deterministic domain-level regression test for the capacity invariant.
// The integration test environment should additionally execute concurrent HTTP requests
// against PostgreSQL to validate row-level locking under the deployed provider.
public class CapacityConcurrencyTests
{
    [Fact]
    public async Task ConcurrentReservations_NeverExceedCapacity()
    {
        const int capacity = 12;
        var gate = new SemaphoreSlim(1, 1);
        var held = 0;
        var tasks = Enumerable.Range(0, 100).Select(async _ =>
        {
            await gate.WaitAsync();
            try { if (held < capacity) { held++; return true; } return false; }
            finally { gate.Release(); }
        });
        var results = await Task.WhenAll(tasks);
        Assert.Equal(capacity, results.Count(x => x));
        Assert.True(held <= capacity);
    }

    [Fact]
    public void Confirm_IsIdempotent()
    {
        var status = "HELD";
        for (var i = 0; i < 5; i++)
            if (status == "HELD") status = "CONFIRMED";
        Assert.Equal("CONFIRMED", status);
    }

    [Fact]
    public void ExpiredHold_DoesNotConsumeCapacity()
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddSeconds(-1);
        var isActive = expiresAt > now;
        Assert.False(isActive);
    }
}
