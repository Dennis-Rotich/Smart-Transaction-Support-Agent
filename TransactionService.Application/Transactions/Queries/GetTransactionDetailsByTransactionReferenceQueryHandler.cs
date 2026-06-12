using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class GetTransactionDetailsByTransactionReferenceQueryHandler : IRequestHandler<GetTransactionDetailsByTransactionReferenceQuery, TransactionDetailResponse?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionDetailsByTransactionReferenceQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionDetailResponse?> Handle(GetTransactionDetailsByTransactionReferenceQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByTransactionReferenceAsync(request.Reference);

        if (transaction == null) return null;

        return new TransactionDetailResponse(
            transaction.MerchantReference,
            transaction.TransactionReference,
            transaction.PaymentMethod,
            transaction.OrderTrackingId,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt,
            transaction.Logs.Select(log => new TransactionLogDto(
                log.TransactionId,
                log.Message,
                log.ProviderResponseCode,
                log.ProviderResponseBody
            )).ToList());
    }
}