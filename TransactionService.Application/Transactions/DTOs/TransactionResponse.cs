namespace TransactionService.Application.Transactions.DTOs;

public record TransactionResponse(
    string Reference,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt);