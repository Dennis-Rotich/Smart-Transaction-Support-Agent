namespace TransactionService.Application.Transactions.DTOs;

public record TransactionLogDto(
    Guid TransactionId,
    string? Message,
    string? ProviderResponseCode,
    string? ProviderResponseBody);