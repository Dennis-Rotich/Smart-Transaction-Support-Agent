using TransactionService.Domain.Enums;
using TransactionService.Domain.Entities;


namespace TransactionService.Domain.Entities
{
    public class Transaction : BaseEntity
    {   
        public string MerchantReference { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public TransactionStatus Status { get; private set; }

        public string? TransactionReference { get; private set; }
        public string? PaymentMethod { get; private set; }
        public string? OrderTrackingId { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public ICollection<TransactionLog> Logs { get; private set; } = new List<TransactionLog>
        ();

        public Transaction(string merchantReference, decimal amount, string currency)
        {
            MerchantReference = merchantReference;
            Amount = amount;
            Currency = currency;
            Status = TransactionStatus.Pending;
        }

        public void MarkAsCompleted(string? transactionReference, string? description, string? paymentMethod)
        {
            Status = TransactionStatus.Completed;
            AddLog(EventType.CallbackReceived, description);
            TransactionReference = transactionReference;
            PaymentMethod = paymentMethod;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsInvalid(string? transactionReference, string? description, string? paymentMethod)
        {
            Status = TransactionStatus.Invalid;
            AddLog(EventType.CallbackReceived, description);
            TransactionReference = transactionReference;
            PaymentMethod = paymentMethod;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsReversed(string? transactionReference, string? description, string? paymentMethod)
        {
            Status = TransactionStatus.Reversed;
            AddLog(EventType.CallbackReceived, description);
            TransactionReference = transactionReference;
            PaymentMethod = paymentMethod;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string? transactionReference, string? description, string? paymentMethod)
        {
            Status = TransactionStatus.Failed;
            AddLog(EventType.CallbackReceived, description);
            TransactionReference = transactionReference;
            PaymentMethod = paymentMethod;
            UpdatedAt = DateTime.UtcNow;
        }

        public void LinkExternalTracking(string orderTrackingId)
        {
            if (string.IsNullOrWhiteSpace(orderTrackingId)) throw new ArgumentException("Tracking ID cannot be empty.");
            OrderTrackingId = orderTrackingId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddLog(EventType type, string? message = null, string? providerResponseCode = null, string? providerResponseBody = null)
        {
            Logs.Add(new TransactionLog(this.Id, type, message, providerResponseCode, providerResponseBody));
        }

    }
}