using MediatR;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class GetTransactionStatusQueryHandler : IRequestHandler<GetTransactionStatusQuery, TransactionResponseGet?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionStatusQueryHandler(ITransactionRepository repository) => _repository = repository;

    public async Task<TransactionResponseGet?> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Querying database for reference: '{request.Reference}'");
        var transaction = await _repository.GetByMerchantReferenceAsync(request.Reference);

        if(transaction == null) return null;

        return new TransactionResponseGet(
            transaction.MerchantReference,
            transaction.TransactionReference,
            transaction.PaymentMethod,
            transaction.OrderTrackingId,
            transaction.UpdatedAt,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt);
    }
}