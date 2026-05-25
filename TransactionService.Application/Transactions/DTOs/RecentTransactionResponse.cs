namespace TransactionService.Application.Transactions.DTOs;

public record RecentTransactionResponse(
    string Reference,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
    );