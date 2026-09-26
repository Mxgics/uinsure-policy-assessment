namespace Uinsure.Domain.Policies;

public sealed class DomainConflictException : Exception
{
    public DomainConflictException(string message) : base(message) { }

    public DomainConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
