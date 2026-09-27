using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.IntegrationTests.Infrastructure;

public sealed class LifecycleSaveBarrier : SaveChangesInterceptor
{
    private readonly TaskCompletionSource _bothReady =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _arrivals;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is not null &&
            context.ChangeTracker.Entries<Policy>().Any(entry => entry.State == EntityState.Modified) &&
            (context.ChangeTracker.Entries<Cancellation>().Any(entry => entry.State == EntityState.Added) ||
             context.ChangeTracker.Entries<PolicyTerm>().Any(entry =>
                 entry.State == EntityState.Added && entry.Entity.PredecessorTermId is not null)))
        {
            if (Interlocked.Increment(ref _arrivals) == 2)
            {
                _bothReady.TrySetResult();
            }
            await _bothReady.Task.WaitAsync(cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
