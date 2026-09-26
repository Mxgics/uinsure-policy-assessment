using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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

    public async Task<CancellationResponse?> QuoteCancellationAsync(
        string reference,
        Guid termId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var policy = await QueryPolicy().SingleOrDefaultAsync(
            item => item.Reference == reference.Trim().ToUpperInvariant(),
            cancellationToken);
        var term = policy?.Terms.SingleOrDefault(item => item.Id == termId);
        return term is null ? null : Map(term.QuoteCancellation(date), null, null);
    }

    public async Task<CancellationResponse?> CancelAsync(
        string reference,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var policy = await QueryTrackedPolicy().SingleOrDefaultAsync(
            item => item.Reference == reference.Trim().ToUpperInvariant(),
            cancellationToken);
        if (policy is null || policy.Terms.All(item => item.Id != termId))
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        var cancellation = policy.Cancel(termId, DateOnly.FromDateTime(now.UtcDateTime), now);
        dbContext.Cancellations.Add(cancellation);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new DomainConflictException("The policy changed while cancellation was being applied.", exception);
        }
        catch (DbUpdateException exception) when (IsCancellationConstraintConflict(exception))
        {
            throw new DomainConflictException("The policy term was cancelled by another request.", exception);
        }
        return Map(cancellation);
    }

    public async Task<PolicyTermResponse?> RenewAsync(
        string reference,
        Guid termId,
        RenewPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await QueryTrackedPolicy().SingleOrDefaultAsync(
            item => item.Reference == reference.Trim().ToUpperInvariant(),
            cancellationToken);
        if (policy is null || policy.Terms.All(item => item.Id != termId))
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        var successor = policy.Renew(
            termId,
            DateOnly.FromDateTime(now.UtcDateTime),
            request.PaymentMethod,
            now);
        dbContext.PolicyTerms.Add(successor);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new DomainConflictException("The policy changed while renewal was being applied.", exception);
        }
        catch (DbUpdateException exception) when (IsRenewalConstraintConflict(exception))
        {
            throw new DomainConflictException("The policy term was renewed by another request.", exception);
        }
        return MapTerm(successor, DateOnly.FromDateTime(now.UtcDateTime));
    }

    private static bool IsCancellationConstraintConflict(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
        sqlException.Message.Contains("IX_Cancellations_PolicyTermId", StringComparison.Ordinal);

    private static bool IsRenewalConstraintConflict(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 } sqlException &&
        sqlException.Message.Contains("IX_PolicyTerms_PredecessorTermId", StringComparison.Ordinal);

    private IQueryable<Policy> QueryPolicy() => dbContext.Policies
        .AsNoTracking()
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Policyholders)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Property)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Payment)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Cancellation)
                .ThenInclude(cancellation => cancellation!.Refund);

    private IQueryable<Policy> QueryTrackedPolicy() => dbContext.Policies
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Policyholders)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Property)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Payment)
        .Include(policy => policy.Terms)
            .ThenInclude(term => term.Cancellation)
                .ThenInclude(cancellation => cancellation!.Refund);

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
        term.PredecessorTermId,
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
            term.Payment.RecordedAtUtc),
        term.Cancellation is null ? null : Map(term.Cancellation));

    private static CancellationResponse Map(Cancellation cancellation) => Map(
        new CancellationCalculation(
            cancellation.EffectiveDate,
            cancellation.RefundAmount,
            cancellation.RetainedPremium,
            "GBP",
            cancellation.Refund?.Method,
            cancellation.Reason,
            cancellation.TotalDays,
            cancellation.UsedDays,
            cancellation.UnusedDays),
        cancellation.RecordedAtUtc,
        cancellation.Refund);

    private static CancellationResponse Map(
        CancellationCalculation calculation,
        DateTimeOffset? recordedAtUtc,
        Refund? refund) => new(
            calculation.Date,
            calculation.RefundAmount,
            calculation.RetainedPremium,
            calculation.Currency,
            calculation.Method,
            calculation.Reason,
            calculation.TotalDays,
            calculation.UsedDays,
            calculation.UnusedDays,
            recordedAtUtc,
            refund is null ? null : new RefundResponse(
                refund.Reference,
                refund.Amount,
                refund.Method,
                refund.RecordedAtUtc));
}
