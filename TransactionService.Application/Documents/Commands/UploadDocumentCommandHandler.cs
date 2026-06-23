using MediatR;
using TransactionService.Application.Documents.DTOs;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Services;

namespace TransactionService.Application.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentUploadResponse>
{   
    private readonly IDocumentRepository _documentRepository;
    private readonly DocumentProcessingService _documentProcessor;

    public UploadDocumentCommandHandler(IDocumentRepository documentRepository, DocumentProcessingService documentProcessor)
    {
        _documentRepository = documentRepository;
        _documentProcessor = documentProcessor;
    }

    public async Task<DocumentUploadResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var fileName = request.FileName;
        
        var documentId = Guid.NewGuid().ToString();

        using var memoryStream = new MemoryStream();
        await request.ContentStream.CopyToAsync(memoryStream, cancellationToken);

        memoryStream.Position = 0;

        try
        {

            await _documentProcessor.ProcessAndStorePdfAsync(documentId, memoryStream);

            return new DocumentUploadResponse
            (
                true,
                $"Successfully vectorized and stored {fileName} in Pinecone!"
            );

        } catch (Exception ex)
        {
            throw new ApplicationException($"Failed to process document for vector storage: {ex.Message}",ex);
        }
    }
}