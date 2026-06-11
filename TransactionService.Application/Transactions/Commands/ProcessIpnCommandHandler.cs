using MediatR;
using TransactionService.Application.Interfaces;

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
        var (statusDesc,
        paymentMethod,
        confirmationCode,
        paymentAccount,
        description) = await _paymentGateway.GetTransactionStatusAsync(token, request.OrderTrackingId);

        var transaction = await _repository.GetByTrackingIdAsync(request.OrderTrackingId);
        if (transaction == null) return false;

        switch(statusDesc.ToUpper())
        {
            case "COMPLETED":
                transaction.MarkAsCompleted(confirmationCode, description);
                break;
            case "FAILED":
                transaction.MarkAsFailed(confirmationCode, description);
                break;
            case "ÏNVALID":
                transaction.MarkAsInvalid(confirmationCode, description);
                break;
            case "REVERSED":
                transaction.MarkAsReversed(confirmationCode, description);
                break;
            default:
                return true; 
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}