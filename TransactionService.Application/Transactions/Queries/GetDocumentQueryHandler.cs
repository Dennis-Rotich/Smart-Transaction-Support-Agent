using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentResultDto?>
{
    private readonly IDocumentRepository _repository;
    public GetDocumentQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }
    public async Task<DocumentResultDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetDocumentByIdAsync(request.DocumentId);
    }
}