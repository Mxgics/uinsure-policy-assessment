namespace Uinsure.Domain.Policies;

public enum PolicyState
{
    Scheduled,
    Current,
    Expired,
    Cancelled
}

public enum PaymentState
{
    NotRecorded,
    Recorded
}
