using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;

namespace TransactionService.Domain.Entities
{
    public class TransactionLog : BaseEntity
    {
        public Guid TransactionId { get; private set; }
        public EventType Type { get; private set; }
        public string? Message { get; private set; }
        public string? ProviderResponseCode { get; private set; }
        public string? ProviderResponseBody { get; private set; }

        public Transaction Transaction { get; private set; } = null!;

        private TransactionLog() { }

        internal TransactionLog(Guid transactionId, EventType type, string? message, string? providerResponseCode, string? providerResponseBody)
        {
            TransactionId = transactionId;
            Type = type;
            Message = message;
            ProviderResponseCode = providerResponseCode;
            ProviderResponseBody = providerResponseBody;
        }
    }
}