using TransactionService.Application.DTOs;

namespace TransactionService.Application.Interfaces;

public interface IQueryRewriterService
{
    Task<string> RewriteQueryAsync(string currentMessage, List<ChatHistoryDTO> chatHistory, CancellationToken cancellationToken = default);
}