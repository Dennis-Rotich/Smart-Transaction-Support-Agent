using MediatR;
using TransactionService.Domain.Repositories;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class GetTransactionStatusQueryHandler : IRequestHandler<GetTransactionStatusQuery, TransactionResponse?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionStatusQueryHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<TransactionResponse?> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Querying database for reference: '{request.Reference}'");
        var transaction = await _repository.GetByReferenceAsync(request.Reference);

        if(transaction == null) return null;

        return new TransactionResponse(
            transaction.Reference,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt);
    }
}