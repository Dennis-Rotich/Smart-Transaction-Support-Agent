using MediatR;
using TransactionService.Application.Documents.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Documents.Commands;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentUploadResponse>
{   
    private readonly IDocumentRepository _documentRepository;

    public UploadDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<DocumentUploadResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var fileName = request.FileName;
        var contentType = request.ContentType;
        using var stream = request.ContentStream;

        return await _documentRepository.CreateDocumentAsync(fileName, contentType, stream);
    }
}