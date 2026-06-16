using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Documents.Queries;

namespace TransactionService.Infrastructure.Tools;

[McpServerToolType]
public class RetrievalTools
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public RetrievalTools(IMediator mediator, ILogger<RetrievalTools> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Searches uploaded system and provider logs for specific errors, events, or keywords. 'dateRange' should be formatted like 'YYYY-MM-DD to YYYY-MM-DD' or 'last 7 days'.")]
    public async Task<string> SearchLogs([Description("A free-text substring search term. It searches across the log's Message, ProviderResponseCode, and Type. Pass an empty string if no text filtering is needed.")] string query, [Description("The time window for the search. MUST be formatted in one of two strict ways: either the exact phrase 'last 7 days', OR a start and end date separated by the word 'to' (e.g., '2023-10-01 to 2023-10-31').")] string dateRange)
    {   
        var safeQuery = query ?? string.Empty;
        var safeDateRange = dateRange ?? "last 7 days";

        var request = new SearchLogsQuery(safeQuery, safeDateRange);
        var result = await _mediator.Send(request);
        
        if(result?.Any() != true)
        {   
            _logger.LogInformation("No logs found matching query '{Query}' in range '{DateRange}'.", safeQuery, safeDateRange);
            return $"No logs found matching query '{query}' in range '{dateRange}'.";
        }

        var formattedLogs = string.Join("\n", result.Select(log => $"[{log.Timestamp:g}] [{log.Level}] {log.Message} (Source: {log.Source})"));

        return $"Search Results for '{query}':\n\n{formattedLogs}";
    }

    [McpServerTool]
    [Description("Retrieves the full text of specific uploaded fintech or system documentation using its Document ID.")]
    public async Task<string> GetDocument([Description("The exact document Id for the specific document, e.g, 2ce63250-b77e-4d8b-a4d3-70209bf8d35f")]string documentId)
    {
        var request = new GetDocumentQuery(documentId);
        var result = await _mediator.Send(request);

        if(result == null)
        {   
            _logger.LogWarning("Document with ID '{DocumentId}' not found in repository.", documentId);
            return $"Error: Document with ID '{documentId}' could not be found in the repository.";
        } 

        return $"--- Document: {result.Title} ---\nVersion: {result.Version}\n\n{result.Content}";
    }

    [McpServerTool]
    [Description("Searches knowledge base for specific articles, or keywords.")]
    public async Task<string> SearchKnowledge([Description("A free-text substring search term. It searches across the knowledge base's Title, Excerpt, and Content. Pass an empty string if no text filtering is needed.")] string query)
    {
        var request = new SearchKnowledgeQuery(query);
        var result = await _mediator.Send(request);

        if(result?.Any() != true)
        {   
            _logger.LogInformation("No knowledge base articles found matching query '{Query}'.", query);
            return $"No knowledge base articles found matching '{query}'.";
        }

        var formattedArticles = string.Join("\n", result.Select(a => $"Title: {a.Title}\nExcerpt: {a.Excerpt}\nContent: {a.Content}\nRelevance Score: {a.Score}"));

        return $"Knowledge Base Results for '{query}':\n\n{formattedArticles}";
    }

    [McpServerTool]
    [Description("Retrieves the full text of specific uploaded fintech or system documentation using a specified query")]
    public async Task<string> SearchDocument([Description("A free-text substring search term. It searches across the document's Title, Version, and Content. Pass an empty string if no text filtering is needed.")]string query)
    {
        var request = new SearchDocumentQuery(query);
        var result = await _mediator.Send(request);

        if (result == null)
        {
            _logger.LogWarning("Document with query '{query}' not found in repository.", query);
            return $"Error: Document with Query '{query}' could not be found in the repository.";
        }

        var formattedDocs = string.Join("\n", result.Select(a => $"Title: {a.Title}\nExcerpt: {a.Version}\nContent: {a.Content}\n"));

        return $"Document Results for '{query}':\n\n{formattedDocs}";
    }
}
