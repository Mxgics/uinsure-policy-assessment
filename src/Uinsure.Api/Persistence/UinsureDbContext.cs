using Microsoft.EntityFrameworkCore;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Persistence;

public sealed class UinsureDbContext(DbContextOptions<UinsureDbContext> options)
    : DbContext(options)
{
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyTerm> PolicyTerms => Set<PolicyTerm>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UinsureDbContext).Assembly);
    }
}
