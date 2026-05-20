using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByReferenceAsync(string referenceId);
    Task<IReadOnlyList<Transaction>> GetRecentTransactionsAsync(int count);
}