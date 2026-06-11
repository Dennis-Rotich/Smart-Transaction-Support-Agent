using TransactionService.Domain.Entities;
using TransactionService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<Transaction?> GetByMerchantReferenceAsync(string merchantReference)
    {
        return await _context.Transactions
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.MerchantReference == merchantReference);
    }

    public async Task<IReadOnlyList<Transaction>> GetRecentTransactionsAsync(int count)
    {
        return await _context.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByTrackingIdAsync(string trackingId)
    {
        return await _context.Transactions
            .Include(t => t.Logs)
            .FirstOrDefaultAsync(t => t.OrderTrackingId == trackingId);
    }
}