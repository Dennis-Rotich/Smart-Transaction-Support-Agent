using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Interfaces;

public interface ISystemLogRepository
{
	Task<IEnumerable<LogResultDto>> SearchLogsAsync(string query, string dateRange);
}

public interface IDocumentRepository
{
	Task<DocumentResultDto?> GetDocumentByIdAsync(string documentId);
}

public interface IKnowledgeBaseRepository
{
	Task<IEnumerable<KnowledgeResultDto>> SearchKnowledgeAsync(string query);
}