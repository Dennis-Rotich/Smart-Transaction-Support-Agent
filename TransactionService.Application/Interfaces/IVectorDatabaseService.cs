using TransactionService.Application.DTOs;

namespace TransactionService.Application.Interfaces;

public interface IVectorDatabaseService
{
    Task UpsertVectorAsync(IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default);
}