using TransactionService.Domain.Enums;
using TransactionService.Domain.Entities;


namespace TransactionService.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public string Reference { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public TransactionStatus Status { get; private set; }
        public string? ProviderReference { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public ICollection<TransactionLog> Logs { get; private set; } = new List<TransactionLog>
        ();

        public Transaction(string reference, decimal amount, string currency)
        {
            Reference = reference;
            Amount = amount;
            Currency = currency;
            Status = TransactionStatus.Pending;
        }

        public void MarkAsSuccess(string providerReference)
        {
            Status = TransactionStatus.Success;
            ProviderReference = providerReference;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? providerReference = null)
        {
            Status = TransactionStatus.Failed;
            ProviderReference = providerReference;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddLog(EventType type, string? message = null, string? providerResponseCode = null, string? providerResponseBody = null)
        {
            Logs.Add(new TransactionLog(this.Id, type, message, providerResponseCode, providerResponseBody));
        }
    }
}