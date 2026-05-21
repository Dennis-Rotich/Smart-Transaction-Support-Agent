namespace TransactionService.Application.Transactions.DTOs;

public record TransactionResponseCreate(
    string Reference,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    string PaymentUrl
    );