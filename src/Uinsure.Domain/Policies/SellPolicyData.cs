namespace Uinsure.Domain.Policies;

public sealed record SellPolicyData(
    InsuranceType Type,
    DateOnly StartDate,
    decimal Premium,
    bool HasClaims,
    bool AutoRenew,
    IReadOnlyList<PolicyholderData> Policyholders,
    PropertyData Property,
    PaymentMethod PaymentMethod);

public sealed record PolicyholderData(string FirstName, string LastName, DateOnly DateOfBirth);

public sealed record PropertyData(
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? City,
    string Postcode);
