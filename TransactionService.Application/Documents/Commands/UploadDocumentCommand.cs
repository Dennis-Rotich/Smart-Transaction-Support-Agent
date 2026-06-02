using MediatR;
using TransactionService.Application.Documents.DTOs;

namespace TransactionService.Application.Documents.Commands;

public record UploadDocumentCommand(string FileName, string ContentType, Stream ContentStream) : IRequest<DocumentUploadResponse>;
