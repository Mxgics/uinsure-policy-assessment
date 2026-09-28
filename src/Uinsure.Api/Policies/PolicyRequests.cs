using System.ComponentModel.DataAnnotations;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Policies;

public sealed class SellPolicyRequest
{
    [Required]
    public InsuranceType? Type { get; init; }
    [Required]
    public DateOnly? StartDate { get; init; }
    [Required]
    public decimal? Premium { get; init; }
    [Required]
    public bool? HasClaims { get; init; }
    [Required]
    public bool? AutoRenew { get; init; }
    [Required]
    public IReadOnlyList<PolicyholderRequest>? Policyholders { get; init; }
    [Required]
    public PropertyRequest? Property { get; init; }
    [Required]
    public PaymentMethod? PaymentMethod { get; init; }
}

public sealed class PolicyholderRequest
{
    [Required]
    public string? FirstName { get; init; }
    [Required]
    public string? LastName { get; init; }
    [Required]
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
