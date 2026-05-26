using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

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