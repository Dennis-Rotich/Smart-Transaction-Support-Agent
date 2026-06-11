namespace TransactionService.Application.Transactions.DTOs;

public record RecentTransactionResponse(
    string MerchantReference,
    string? TransactionReference,
    string? PaymentMethod,
    string? OrderTrackingId,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
    );

