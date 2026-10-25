using System.Threading.RateLimiting;     // PartitionedRateLimiter, RateLimitPartition, SlidingWindowRateLimiterOptions
using Microsoft.Extensions.Options;      // IOptions
using PetStore.Domain.Interfaces;        // ILoginAttemptLimiter
using PetStore.Domain.Settings;          // RateLimitSettings

namespace PetStore.Infrastructure.Security;

public class LoginAttemptLimiter : ILoginAttemptLimiter, IDisposable
{
    private readonly PartitionedRateLimiter<string> _rateLimiter;
    public int RetryAfterSeconds { get; }

    public LoginAttemptLimiter(IOptions<RateLimitSettings> settings)
    {
        var options = settings.Value;
        RetryAfterSeconds = options.EmailWindowMinutes * 60;

        _rateLimiter = PartitionedRateLimiter.Create<string, string>(email =>
            RateLimitPartition.GetSlidingWindowLimiter(email, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = options.EmailPermitLimit,
                Window = TimeSpan.FromMinutes(options.EmailWindowMinutes),
                SegmentsPerWindow = 5,
                QueueLimit = 0
            }));
    }

    public bool IsBlocked(string email)
    {
        using var lease = _rateLimiter.AttemptAcquire(email, permitCount: 0);
        return !lease.IsAcquired;
    }

    public void RecordFailure(string email)
    {
        _rateLimiter.AttemptAcquire(email, permitCount: 1).Dispose();
    }

    public void Dispose() => _rateLimiter.Dispose();
}