namespace Uinsure.Domain.Policies;

public static class PolicyLimits
{
    public const int Name = 100;
    public const int AddressLine = 200;
    public const int City = 100;
    public const int Postcode = 8;
    public const decimal MaximumPremium = 9999999999999999.99m;
}
