namespace PetStore.Domain.Exceptions;

public class TooManyRequestsException : Exception
{
    public int RetryAfterSeconds { get; }

    public TooManyRequestsException(string message, int retryAfterSeconds) : base(message)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}    