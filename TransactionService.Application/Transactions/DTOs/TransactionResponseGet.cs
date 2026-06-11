namespace TransactionService.Application.Transactions.DTOs;

public record TransactionResponseGet(
    string MerchantReference,
    string? TransactionReference,
    string? PaymentMethod,
    string? OrderTrackingId,
    DateTime? UpdatedAt,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
    );