using Uinsure.Domain;

namespace Uinsure.Domain.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_does_not_reference_web_or_persistence_frameworks()
    {
        var references = typeof(AssemblyMarker).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToArray();

        Assert.DoesNotContain(references, name => name!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name!.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }
}
