using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionService.Domain.Repositories;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class  ListRecentTransactionsQueryHandler : IRequestHandler<ListRecentTransactionsQuery, IEnumerable<RecentTransactionResponse>>
{
    private readonly ITransactionRepository _repository;

    public ListRecentTransactionsQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RecentTransactionResponse>> Handle (ListRecentTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetRecentTransactionsAsync(request.Limit);

        return transactions.Select(t => new RecentTransactionResponse(
            t.Reference,
            t.Amount,
            t.Currency,
            t.Status.ToString(),
            t.CreatedAt));
    }
}

