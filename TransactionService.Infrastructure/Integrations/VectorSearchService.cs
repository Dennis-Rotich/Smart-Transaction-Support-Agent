using System.Text;
using Pinecone;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class VectorSearchService : IVectorSearchService
{
    private readonly PineconeClient _pineconeClient;
    private readonly IAiOrchestratorService _aiOrchestrator;
    private readonly ILogger<VectorSearchService> _logger;

    private const string IndexName = "api-docs";

    public VectorSearchService(PineconeClient pineconeClient, IAiOrchestratorService aiOrchestrator, ILogger<VectorSearchService> logger)
    {
        _pineconeClient = pineconeClient;
        _aiOrchestrator = aiOrchestrator;
        _logger = logger;
    }

    public async Task<string> SearchContextAsync(string rewrittenQuery)
    {
        if(string.IsNullOrWhiteSpace(rewrittenQuery) || rewrittenQuery == "NO_SEARCH") return string.Empty;

        try
        {
            _logger.LogInformation("Generating embedding for query: {Query}", rewrittenQuery);
            var embeddings = await _aiOrchestrator.GenerateEmbeddingsAsync(new List<string> { rewrittenQuery });
            var queryVector = embeddings.FirstOrDefault();

            if (queryVector == null) 
            {
                _logger.LogWarning("OpenAI failed to generate a vector for the query.");
                return string.Empty;
            } 

            var index = await _pineconeClient.GetIndex(IndexName);

            _logger.LogInformation("Executing Pinecone Query with TopK = 5...");
            var matches = await index.Query(
                values: queryVector,
                topK: 5u,
                includeMetadata: true);

            if (matches == null || !matches.Any())
            {
                _logger.LogWarning("Pinecone returned 0 matches for the query.");
                return string.Empty;
            }

            _logger.LogInformation("Pinecone returned {Count} matches. Extracting metadata...", matches.Count());

            var contextBuilder = new StringBuilder();

            foreach (var match in matches)
            {
                _logger.LogInformation("Match Score: {Score}", match.Score);

                if (match.Metadata != null && match.Metadata.TryGetValue("Text", out var textValue))
                {
                    var actualText = textValue.Inner?.ToString() ?? textValue.ToString();

                    if (!string.IsNullOrWhiteSpace(actualText))
                    {
                        contextBuilder.AppendLine(actualText);
                        contextBuilder.AppendLine("\n---\n");
                    }
                }
                else
                {
                    _logger.LogWarning("A match was found, but the 'Text' metadata key was missing.");
                }
            }

            var finalContext = contextBuilder.ToString().Trim();
            _logger.LogInformation("Successfully compiled {Length} characters of context.", finalContext.Length);

            return finalContext;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to execute vector search: {ex.Message}", ex);
        }
    }
}