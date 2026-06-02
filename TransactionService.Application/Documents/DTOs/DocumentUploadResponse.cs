
namespace TransactionService.Application.Documents.DTOs;

public record DocumentUploadResponse(string FileName, string StoragePath, string DocumentType);