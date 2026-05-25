using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TransactionService.Domain.Repositories;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public class GetTransactionDetailsQueryHandler : IRequestHandler<GetTransactionDetailsQuery, TransactionDetailResponse?>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionDetailsQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionDetailResponse?> Handle (GetTransactionDetailsQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByReferenceAsync(request.Reference);

        if (transaction == null) return null;

        return new TransactionDetailResponse(
            transaction.Reference,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt,
            transaction.Logs.Select(log => new TransactionLogDto(
                log.TransactionId,          
                log.Message,      
                log.ProviderResponseCode,
                log.ProviderResponseBody
            )).ToList(),
            transaction.ExternalTrackingId);
    }
}