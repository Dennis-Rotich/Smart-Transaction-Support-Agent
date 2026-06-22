using System.Text;
using Pinecone;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class VectorSearchService : IVectorSearchService
{
    private readonly PineconeClient _pineconeClient;
    private readonly IAiOrchestratorService _aiOrchestrator;

    private const string IndexName = "api-docs";

    public VectorSearchService(PineconeClient pineconeClient, IAiOrchestratorService aiOrchestrator)
    {
        _pineconeClient = pineconeClient;
        _aiOrchestrator = aiOrchestrator;
    }

    public async Task<string> SearchContextAsync(string rewrittenQuery)
    {
        if(string.IsNullOrWhiteSpace(rewrittenQuery) || rewrittenQuery == "NO_SEARCH") return string.Empty;

        try
        {
            var embeddings = await _aiOrchestrator.GenerateEmbeddingsAsync(new List<string> { rewrittenQuery });
            var queryVector = embeddings.FirstOrDefault();

            if (queryVector != null) return string.Empty;

            var index = await _pineconeClient.GetIndex(IndexName);

            var queryResponse = await index.Query(
                values: queryVector,
                topK: 5u,
                includeMetadata: true);

            var contextBuilder = new StringBuilder();

            if(queryResponse != null && queryResponse.Any())
            {
                foreach(var match in queryResponse)
                {
                    if(match.Metadata != null && match.Metadata.TryGetValue("Text", out var textValue))
                    {
                        contextBuilder.AppendLine(textValue.ToString());

                        contextBuilder.AppendLine("\n---\n");
                    }
                }
            }

            return contextBuilder.ToString().Trim();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to execute vector search: {ex.Message}", ex);
        }
    }
}