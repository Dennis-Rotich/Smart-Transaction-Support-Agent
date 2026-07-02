using TransactionService.Application.Utilities;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;


namespace TransactionService.Application.Services;

public class DocumentProcessingService
{
    private readonly IPdfExtractionService _pdfExtractor;
    private readonly IAiOrchestratorService _openAi;
    private readonly IVectorDatabaseService _vectorDb;

    public DocumentProcessingService(IPdfExtractionService pdfExtractor, IAiOrchestratorService openAi, IVectorDatabaseService vectorDb)
    {
        _pdfExtractor = pdfExtractor;
        _openAi = openAi;
        _vectorDb = vectorDb;
    }

    public async Task ProcessAndStorePdfAsync(string documentId, Stream pdfStream)
    {
        string rawText = await _pdfExtractor.ExtractTextAsync(pdfStream);

        List<string> chunks = TextChunker.SplitText(rawText);

        if (!chunks.Any()) return;

        List<float[]> vectors = await _openAi.GenerateEmbeddingsAsync(chunks);

        var recordsToSave = new List<VectorRecord>();

        for(int i = 0; i < chunks.Count; i++)
        {
            string chunkId = $"{documentId}_chunk_{i}";

            var record = new VectorRecord(
                Id:chunkId,
                Values: vectors[i],
                Text: chunks[i],
                DocumentId: documentId);

            recordsToSave.Add(record);
        }

        await _vectorDb.UpsertVectorAsync(recordsToSave);
    }
}