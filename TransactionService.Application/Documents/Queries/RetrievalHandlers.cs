using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Documents.DTOs;

namespace TransactionService.Application.Documents.Queries;

public class SearchLogsQueryHandler : IRequestHandler<SearchLogsQuery, IEnumerable<LogResultDto>>
{
    private readonly ISystemLogRepository _repository;

    public SearchLogsQueryHandler(ISystemLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<LogResultDto>> Handle(SearchLogsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchLogsAsync(request.Query, request.DateRange);
    }
}

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

public class SearchDocumentQueryHandler : IRequestHandler<SearchDocumentQuery, IEnumerable<DocumentResultDto>>
{
    private readonly IDocumentRepository _repository;
    public SearchDocumentQueryHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<DocumentResultDto>> Handle(SearchDocumentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchDocumentAsync(request.Query);
    }
}

public class SearchKnowledgeQueryHandler : IRequestHandler<SearchKnowledgeQuery, IEnumerable<KnowledgeResultDto>>
{
    private readonly IKnowledgeBaseRepository _repository;
    public SearchKnowledgeQueryHandler(IKnowledgeBaseRepository repository)
    {
        _repository = repository;
    }
    public async Task<IEnumerable<KnowledgeResultDto>> Handle(SearchKnowledgeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.SearchKnowledgeAsync(request.Query);
    }
}