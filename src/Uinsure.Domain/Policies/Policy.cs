namespace Uinsure.Domain.Policies;

public sealed class Policy
{
    private readonly List<PolicyTerm> _terms = [];

    private Policy() { }

    private Policy(Guid id, string reference, InsuranceType type)
    {
        Id = id;
        Reference = reference;
        Type = type;
    }

    public Guid Id { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public InsuranceType Type { get; private set; }
    public long MutationRevision { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<PolicyTerm> Terms => _terms;

    public static Policy Sell(
        string reference,
        SellPolicyData data,
        DateOnly today,
        DateTimeOffset recordedAtUtc)
    {
        var errors = Validate(data, today);
        if (errors.Count > 0)
        {
            throw new DomainValidationException(errors);
        }

        var policy = new Policy(Guid.NewGuid(), reference.Trim().ToUpperInvariant(), data.Type);
        policy._terms.Add(PolicyTerm.CreateInitial(policy.Id, data, recordedAtUtc));
        return policy;
    }

    private static Dictionary<string, string[]> Validate(SellPolicyData data, DateOnly today)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var latestStart = today.AddDays(60);
        if (data.StartDate < today || data.StartDate > latestStart)
        {
            errors["startDate"] = ["Start date must be from today through 60 days ahead."];
        }

        if (data.Premium <= 0 || decimal.Round(data.Premium, 2) != data.Premium)
        {
            errors["premium"] = ["Premium must be positive with no more than two decimal places."];
        }

        if (data.Policyholders.Count is < 1 or > 3)
        {
            errors["policyholders"] = ["Provide between one and three policyholders."];
        }

        for (var index = 0; index < data.Policyholders.Count; index++)
        {
            var holder = data.Policyholders[index];
            if (string.IsNullOrWhiteSpace(holder.FirstName) || string.IsNullOrWhiteSpace(holder.LastName))
            {
                errors[$"policyholders[{index}].name"] = ["First and last name are required."];
            }
            if (holder.DateOfBirth > today || !IsAtLeastSixteen(holder.DateOfBirth, data.StartDate))
            {
                errors[$"policyholders[{index}].dateOfBirth"] = ["Policyholders must be at least 16 on the start date."];
            }
        }

        if (string.IsNullOrWhiteSpace(data.Property.AddressLine1) || string.IsNullOrWhiteSpace(data.Property.City))
        {
            errors["property.address"] = ["Address line 1 and city are required."];
        }
        var postcode = data.Property.Postcode.Trim();
        if (postcode.Length is < 1 or > 8)
        {
            errors["property.postcode"] = ["Postcode is required and must be at most eight characters."];
        }
        if (data.Property.Bedrooms <= 0)
        {
            errors["property.bedrooms"] = ["Bedrooms must be positive."];
        }

        return errors;
    }

    private static bool IsAtLeastSixteen(DateOnly dateOfBirth, DateOnly startDate)
    {
        try
        {
            return dateOfBirth.AddYears(16) <= startDate;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }
}
