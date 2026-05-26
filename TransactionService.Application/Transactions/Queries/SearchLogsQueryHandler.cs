using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class  SearchLogsQueryHandler : IRequestHandler<SearchLogsQuery, IEnumerable<LogResultDto>>
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