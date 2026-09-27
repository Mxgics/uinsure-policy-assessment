namespace Uinsure.Domain.Policies;

public enum CancellationReason
{
    NoPayment,
    HasClaims,
    BeforeStart,
    CoolingOff,
    ProRata
}
