using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using ModelContextProtocol.Server;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Infrastructure.Tools;

[McpServerToolType]
public class RetrievalTools
{
    private readonly IMediator _mediator;

    public RetrievalTools(IMediator mediator)
    {
        _mediator = mediator;
    }

    [McpServerTool]
    [Description("Searches uploaded system and provider logs for specific errors, events, or keywords. 'dateRange' should be formatted like 'YYYY-MM-DD to YYYY-MM-DD' or 'last 7 days'.")]
    public async Task<string> SearchLogs(string query, string dateRange)
    {
        var request = new SearchLogsQuery(query, dateRange);
        var result = await _mediator.Send(request);
        
        if(result?.Any() != true)
        {
            return $"No logs found matching query '{query}' in range '{dateRange}'.";
        }

        var formattedLogs = string.Join("\n", result.Select(log => $"[{log.Timestamp:g}] [{log.Level}] {log.Message} (Source: {log.Source})"));

        return $"Search Results for '{query}':\n\n{formattedLogs}";
    }

    [McpServerTool]
    [Description("Retrieves the full text of specific uploaded fintech or system documentation using its Document ID.")]
    public async Task<string> GetDocument(string documentId)
    {
        var request = new GetDocumentQuery(documentId);
        var result = await _mediator.Send(request);

        if(result == null) return $"Error: Document with ID '{documentId}' could not be found in the repository.";

        return $"--- Document: {result.Title} ---\nVersion: {result.Version}\n\n{result.Content}";
    }

    [McpServerTool]
    [Description("Searches knowledge base for specific articles, or keywords.")]
    public async Task<string> SearchKnowledge(string query)
    {
        var request = new SearchKnowledgeQuery(query);
        var result = await _mediator.Send(request);

        if(result?.Any() != true) return $"No knowledge base articles found matching '{query}'.";

        var formattedArticles = string.Join("\n", result.Select(a => $"Title: {a.Title}\nExcerpt: {a.Excerpt}\nRelevance Score: {a.Score}"));

        return $"Knowledge Base Results for '{query}':\n\n{formattedArticles}";
    }
}
