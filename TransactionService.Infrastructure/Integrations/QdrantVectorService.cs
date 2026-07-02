using Qdrant.Client;
using Qdrant.Client.Grpc;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class QdrantVectorService : IVectorDatabaseService
{
    private readonly QdrantClient _client;
    private const string CollectionName = "api-docs";

    public QdrantVectorService()
    {
        _client = new QdrantClient("localhost", 6334);
    }

    public async Task UpsertVectorAsync(IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        if (records == null || !records.Any()) return;

        try
        {
            var points = new List<PointStruct>();

            foreach (var record in records)
            {
                var point = new PointStruct
                {
                    Id = Guid.TryParse(record.Id, out var parsedId) ? parsedId : Guid.NewGuid(),
                    Vectors = record.Values,

                    Payload =
                    {
                        ["DocumentId"] = record.DocumentId,
                        ["Text"] = record.Text,
                    }
                };
                points.Add(point);
            }

            await _client.UpsertAsync(CollectionName, points, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to upsert vectors to Qdrant: {ex.Message}", ex);
        }

    }
}