using Microsoft.EntityFrameworkCore;
using Uinsure.Api.Persistence;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Policies;

public sealed class PolicyService(
    UinsureDbContext dbContext,
    TimeProvider timeProvider,
    IPolicyReferenceGenerator referenceGenerator)
{
    public async Task<PolicyResponse> SellAsync(
        SellPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var data = MapAndValidate(request);
        var policy = Policy.Sell(
            referenceGenerator.Create(),
            data,
            DateOnly.FromDateTime(now.UtcDateTime),
            now);

        dbContext.Policies.Add(policy);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(policy, DateOnly.FromDateTime(now.UtcDateTime));
    }

    public async Task<PolicyResponse?> GetAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var normalized = reference.Trim().ToUpperInvariant();
        var policy = await QueryPolicy()
            .SingleOrDefaultAsync(item => item.Reference == normalized, cancellationToken);
        return policy is null ? null : Map(policy, UtcToday());
    }

    public async Task<PolicyTermResponse?> GetTermAsync(
        string reference,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var policy = await QueryPolicy()
            .SingleOrDefaultAsync(
                item => item.Reference == reference.Trim().ToUpperInvariant(),
                cancellationToken);
        if (policy is null)
        {
            return null;
        }

        var term = policy.Terms.SingleOrDefault(item => item.Id == termId);
        return term is null ? null : MapTerm(term, UtcToday());
    }

    private IQueryable<Policy> QueryPolicy() => dbContext.Policies
        .AsNoTracking()
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Policyholders)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Property)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Payment);

    private DateOnly UtcToday() =>
        DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

    private static SellPolicyData MapAndValidate(SellPolicyRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (request.Type is null) errors["type"] = ["Insurance type is required."];
        if (request.StartDate is null) errors["startDate"] = ["Start date is required."];
        if (request.Premium is null) errors["premium"] = ["Premium is required."];
        if (request.HasClaims is null) errors["hasClaims"] = ["Has claims is required."];
        if (request.AutoRenew is null) errors["autoRenew"] = ["Auto renew is required."];
        if (request.PaymentMethod is null) errors["paymentMethod"] = ["Payment method is required."];
        if (request.Policyholders is null) errors["policyholders"] = ["Policyholders are required."];
        if (request.Property is null) errors["property"] = ["Property is required."];
        if (request.Policyholders is not null)
        {
            for (var index = 0; index < request.Policyholders.Count; index++)
            {
                if (request.Policyholders[index].DateOfBirth is null)
                {
                    errors[$"policyholders[{index}].dateOfBirth"] = ["Date of birth is required."];
                }
            }
        }
        if (request.Property is not null && request.Property.Bedrooms is null)
        {
            errors["property.bedrooms"] = ["Bedrooms is required."];
        }
        if (errors.Count > 0) throw new DomainValidationException(errors);

        return new SellPolicyData(
            request.Type!.Value,
            request.StartDate!.Value,
            request.Premium!.Value,
            request.HasClaims!.Value,
            request.AutoRenew!.Value,
            request.Policyholders!.Select(holder => new PolicyholderData(
                holder.FirstName ?? string.Empty,
                holder.LastName ?? string.Empty,
                holder.DateOfBirth!.Value)).ToArray(),
            new PropertyData(
                request.Property!.AddressLine1 ?? string.Empty,
                request.Property.AddressLine2,
                request.Property.City ?? string.Empty,
                request.Property.Postcode ?? string.Empty,
                request.Property.Bedrooms!.Value),
            request.PaymentMethod!.Value);
    }

    private static PolicyResponse Map(Policy policy, DateOnly today) => new(
        policy.Reference,
        policy.Type,
        policy.Terms
            .OrderBy(term => term.StartDate)
            .ThenBy(term => term.Id)
            .Select(term => MapTerm(term, today))
            .ToArray());

    private static PolicyTermResponse MapTerm(PolicyTerm term, DateOnly today) => new(
        term.Id,
        term.StartDate,
        term.EndDate,
        term.Premium,
        term.HasClaims,
        term.AutoRenew,
        term.StateOn(today),
        term.Payment is null ? PaymentState.NotRecorded : PaymentState.Recorded,
        term.Policyholders.Select(holder => new PolicyholderResponse(
            holder.FirstName,
            holder.LastName,
            holder.DateOfBirth)).ToArray(),
        new PropertyResponse(
            term.Property.AddressLine1,
            term.Property.AddressLine2,
            term.Property.City,
            term.Property.Postcode,
            term.Property.Bedrooms),
        term.Payment is null ? null : new PaymentResponse(
            term.Payment.Reference,
            term.Payment.Method,
            term.Payment.Amount,
            term.Payment.RecordedAtUtc));
}
