namespace Uinsure.Domain.Policies;

public sealed class PolicyholderSnapshot
{
    private PolicyholderSnapshot() { }

    public Guid Id { get; private set; }
    public Guid PolicyTermId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }

    internal static PolicyholderSnapshot Create(Guid termId, PolicyholderData data) => new()
    {
        Id = Guid.NewGuid(),
        PolicyTermId = termId,
        FirstName = data.FirstName.Trim(),
        LastName = data.LastName.Trim(),
        DateOfBirth = data.DateOfBirth
    };
}
