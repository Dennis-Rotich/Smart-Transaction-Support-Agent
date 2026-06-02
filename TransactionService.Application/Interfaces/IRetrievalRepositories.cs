using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TransactionService.Application.Transactions.DTOs;
using TransactionService.Application.Documents.DTOs;

namespace TransactionService.Application.Interfaces;

public interface ISystemLogRepository
{
	Task<IEnumerable<LogResultDto>> SearchLogsAsync(string query, string dateRange);
}

public interface IDocumentRepository
{
	Task<DocumentResultDto?> GetDocumentByIdAsync(string documentId);
	Task<DocumentUploadResponse> CreateDocumentAsync(string fileName, string contentType, Stream fileStream);
}

public interface IKnowledgeBaseRepository
{
	Task<IEnumerable<KnowledgeResultDto>> SearchKnowledgeAsync(string query);
}