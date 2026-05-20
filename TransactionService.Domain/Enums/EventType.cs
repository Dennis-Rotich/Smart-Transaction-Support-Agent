namespace TransactionService.Domain.Enums
{
    public enum EventType
    {
        Creation,
        StatusPoll,
        CallbackReceived,
        SystemError
    }
}