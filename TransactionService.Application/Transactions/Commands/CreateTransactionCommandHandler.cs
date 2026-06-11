using MediatR;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Application.Transactions.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Transactions.Commands;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponseCreate>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentGateway;

    public CreateTransactionCommandHandler(ITransactionRepository repository, IUnitOfWork unitOfWork, IPaymentGatewayService paymentGateway)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<TransactionResponseCreate> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var reference = $"TXN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        var transaction = new Transaction(reference, request.Amount, request.Currency);

        transaction.AddLog(EventType.Creation, "Payment order initialized by API.");

        await _repository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var pesapalToken = await _paymentGateway.GetTokenAsync();

        var (checkoutUrl, orderTrackingId) = await _paymentGateway.SubmitOrderAsync(pesapalToken, transaction.Amount, transaction.Currency, transaction.MerchantReference);

        transaction.LinkExternalTracking(orderTrackingId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransactionResponseCreate(
            transaction.MerchantReference,
            transaction.Amount,
            transaction.Currency,
            transaction.Status.ToString(),
            transaction.CreatedAt,
            checkoutUrl);
    }
}