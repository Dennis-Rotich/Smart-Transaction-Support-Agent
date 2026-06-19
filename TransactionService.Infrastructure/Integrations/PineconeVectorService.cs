using Pinecone;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class PineconeVectorService : IVectorDatabaseService
{
    private readonly PineconeClient _pineconeClient;

    private const string IndexName = "api-docs";

    public PineconeVectorService(PineconeClient pineconeClient)
    {
        _pineconeClient = pineconeClient;
    }

    public async Task UpsertVectorAsync(IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        if (records == null || !records.Any()) return;

        try
        {
            var index = await _pineconeClient.GetIndex(IndexName);

            var pineconeVectors = new List<Vector>();

            foreach (var record in records)
            {
                var metadata = new MetadataMap
                {
                    ["DocumentId"] = record.DocumentId,
                    ["Text"] = record.Text
                };

                var vector = new Vector
                {
                    Id = record.Id,
                    Values = record.Values,
                    Metadata = metadata
                };

                pineconeVectors.Add(vector);
            }

            await index.Upsert(pineconeVectors);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to upsert vectors to Pinecone: {ex.Message}", ex);
        }
    }
}