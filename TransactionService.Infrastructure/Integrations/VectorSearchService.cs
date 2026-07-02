using System.Text;
//using Pinecone;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class VectorSearchService : IVectorSearchService
{
    //private readonly PineconeClient _pineconeClient;
    private readonly QdrantClient _qdrantClient;
    private readonly IAiOrchestratorService _aiOrchestrator;
    private readonly ILogger<VectorSearchService> _logger;

    private const string CollectionName = "api-docs";

    public VectorSearchService(/*PineconeClient pineconeClient*/ IAiOrchestratorService aiOrchestrator, ILogger<VectorSearchService> logger)
    {
        //_pineconeClient = pineconeClient;
        _qdrantClient = new QdrantClient("localhost", 6334);
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

            _logger.LogInformation("Executing Qdrant Query with Limit = 5...");

            var matches = await _qdrantClient.SearchAsync(
                collectionName: CollectionName,
                vector:queryVector,
                limit:5,
                payloadSelector:true);

            if (matches == null || !matches.Any())
            {
                _logger.LogWarning("Qdrant returned 0 matches for the query.");
                return string.Empty;
            }

            _logger.LogInformation("Qdrant returned {Count} matches. Extracting payload...", matches.Count());

            var contextBuilder = new StringBuilder();

            foreach (var match in matches)
            {
                _logger.LogInformation("Match Score: {Score}", match.Score);

                if (match.Payload != null && match.Payload.TryGetValue("Text", out var textValue))
                {
                    var actualText = textValue.StringValue;

                    if (!string.IsNullOrWhiteSpace(actualText))
                    {
                        contextBuilder.AppendLine(actualText);
                        contextBuilder.AppendLine("\n---\n");
                    }
                }
                else
                {
                    _logger.LogWarning("A match was found, but the 'Text' payload key was missing.");
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