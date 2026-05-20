using MediatR;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.Repositories;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransactionCommandHandler(ITransactionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var reference = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var transaction = new Transaction(reference, request.Amount, request.Currency);

        transaction.AddLog(EventType.Creation, "Payment order initialized by API.");

        await _repository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransactionResponse(
            transaction.Reference,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt);
    }
}