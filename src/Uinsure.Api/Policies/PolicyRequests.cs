using System.ComponentModel.DataAnnotations;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Policies;

public sealed class SellPolicyRequest
{
    public InsuranceType? Type { get; init; }
    public DateOnly? StartDate { get; init; }
    public decimal? Premium { get; init; }
    public bool? HasClaims { get; init; }
    public bool? AutoRenew { get; init; }
    public IReadOnlyList<PolicyholderRequest>? Policyholders { get; init; }
    public PropertyRequest? Property { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
}

public sealed class PolicyholderRequest
{
    [Required]
    public string? FirstName { get; init; }
    [Required]
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
}

public sealed class PropertyRequest
{
    [Required]
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    [Required]
    public string? Postcode { get; init; }
}

public sealed record RenewPolicyRequest(PaymentMethod? PaymentMethod);
