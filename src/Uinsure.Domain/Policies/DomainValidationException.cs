namespace Uinsure.Domain.Policies;

public sealed class DomainValidationException(
    IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more business validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
