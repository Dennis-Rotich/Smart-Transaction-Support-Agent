using MediatR;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Repositories;

namespace TransactionService.Application.Transactions.Commands;

public class ProcessIpnCommandHandler : IRequestHandler<ProcessIpnCommand, bool>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentGateway;

    public ProcessIpnCommandHandler(ITransactionRepository repository, IUnitOfWork unitOfWork, IPaymentGatewayService paymentGateway)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
    }

    public async Task<bool> Handle(ProcessIpnCommand request, CancellationToken cancellationToken)
    {
        var token = await _paymentGateway.GetTokenAsync();
        var (paymentStatus, providerReference) = await _paymentGateway.GetTransactionStatusAsync(token, request.OrderTrackingId);

        var transaction = await _repository.GetByTrackingIdAsync(request.OrderTrackingId);
        if (transaction == null) return false;

        switch(paymentStatus.ToUpper())
        {
            case "COMPLETED":
                transaction.MarkAsSuccess(providerReference);
                break;
            case "FAILED":
            case "ÏNVALID":
                transaction.MarkAsFailed(providerReference);
                break;
            default:
                return true; 
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}