namespace TransactionService.Application.DTOs;

public record VectorSearchResult(
    string Id,
    float Score,
    string Text,
    string DocumentId);