using Microsoft.EntityFrameworkCore;

namespace Uinsure.Api.Persistence;

public sealed class UinsureDbContext(DbContextOptions<UinsureDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UinsureDbContext).Assembly);
    }
}
