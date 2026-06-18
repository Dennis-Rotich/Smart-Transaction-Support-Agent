namespace TransactionService.Application.DTOs;

public record VectorRecord(
    string Id,
    float[] Values,
    string Text,
    string DocumentId);