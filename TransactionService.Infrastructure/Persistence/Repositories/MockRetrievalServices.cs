using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;

public class MockRetrievalServices : ISystemLogRepository, IDocumentRepository, IKnowledgeBaseRepository
{
    public Task<IEnumerable<LogResultDto>> SearchLogsAsync(string query, string dateRange)
    {
        var logs = new List<LogResultDto>
        {
            new (DateTime.UtcNow.AddMinutes(-5), "ERROR", "Pesapal API Timeout on CreateOrder", "TransactionService"),
            new (DateTime.UtcNow.AddMinutes(-10), "INFO", "Webhook received for TEST-MCP-001", "IpnListener")
        };

        return Task.FromResult(logs.Where(l => l.Message.Contains(query, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<DocumentResultDto?> GetDocumentByIdAsync(string documentId)
    {
        if (documentId == "DOC-101")
        {
            return Task.FromResult<DocumentResultDto?>(
                new DocumentResultDto("Pesapal Webhook Integration", "1.2", "To integrate webhooks, you must register your IPN URL..."));
        }
        return Task.FromResult<DocumentResultDto?>(null);
    }

    public Task<IEnumerable<KnowledgeResultDto>> SearchKnowledgeAsync(string query)
    {
        var articles = new List<KnowledgeResultDto>
        {
            new ("Handling Duplicate IPNs", "If Pesapal sends a duplicate webhook, ignore it if the status hasn't changed.", 0.95),
            new ("Refund Policies", "Refunds must be initiated within 7 days of the original transaction.", 0.82)
        };
        return Task.FromResult(articles.AsEnumerable());
    }
}