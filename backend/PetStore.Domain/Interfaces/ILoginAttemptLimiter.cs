namespace PetStore.Domain.Interfaces;

public interface ILoginAttemptLimiter
{
    bool IsBlocked(string email);
    void RecordFailure(string email);
    int RetryAfterSeconds { get; }
}