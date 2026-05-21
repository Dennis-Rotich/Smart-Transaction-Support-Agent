namespace TransactionService.Application.Transactions.DTOs;

public record TransactionResponseGet(
    string Reference,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
    );