using TransactionService.Domain.Entities;

namespace TransactionService.Application.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByMerchantReferenceAsync(string merchantReference);
    Task<IReadOnlyList<Transaction>> GetRecentTransactionsAsync(int count);
    Task<Transaction?> GetByTrackingIdAsync(string trackingId);
}