namespace TransactionService.Application.Interfaces;

public interface IVectorSearchService
{
    Task<string> SearchContextAsync(string rewrittenQuery);
}