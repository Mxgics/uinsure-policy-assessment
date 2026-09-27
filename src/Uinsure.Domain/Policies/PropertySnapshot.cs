namespace Uinsure.Domain.Policies;

public sealed class PropertySnapshot
{
    private PropertySnapshot() { }

    public Guid Id { get; private set; }
    public Guid PolicyTermId { get; private set; }
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string Postcode { get; private set; } = string.Empty;
    public int Bedrooms { get; private set; }

    internal static PropertySnapshot Create(Guid termId, PropertyData data) => new()
    {
        Id = Guid.NewGuid(),
        PolicyTermId = termId,
        AddressLine1 = data.AddressLine1.Trim(),
        AddressLine2 = string.IsNullOrWhiteSpace(data.AddressLine2) ? null : data.AddressLine2.Trim(),
        City = data.City.Trim(),
        Postcode = data.Postcode.Trim().ToUpperInvariant(),
        Bedrooms = data.Bedrooms
    };
}
