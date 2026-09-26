using Uinsure.Domain.Policies;

namespace Uinsure.Api.Policies;

public sealed record PolicyResponse(
    string Reference,
    InsuranceType Type,
    IReadOnlyList<PolicyTermResponse> Terms);

public sealed record PolicyTermResponse(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Premium,
    bool HasClaims,
    bool AutoRenew,
    PolicyState State,
    PaymentState PaymentState,
    IReadOnlyList<PolicyholderResponse> Policyholders,
    PropertyResponse Property,
    PaymentResponse? Payment);

public sealed record PolicyholderResponse(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);

public sealed record PropertyResponse(
    string AddressLine1,
    string? AddressLine2,
    string City,
    string Postcode,
    int Bedrooms);

public sealed record PaymentResponse(
    string Reference,
    PaymentMethod Method,
    decimal Amount,
    DateTimeOffset RecordedAtUtc);
