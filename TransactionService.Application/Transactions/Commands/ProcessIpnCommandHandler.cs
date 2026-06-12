using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.Transactions.Commands;

public class ProcessIpnCommandHandler : IRequestHandler<ProcessIpnCommand, bool>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentGatewayService _paymentGateway;

    private readonly ILogger<ProcessIpnCommandHandler> _logger;

    public ProcessIpnCommandHandler(ITransactionRepository repository, IUnitOfWork unitOfWork, IPaymentGatewayService paymentGateway, ILogger<ProcessIpnCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _paymentGateway = paymentGateway;
        _logger = logger;
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

        string incomingStatus = statusDesc.ToUpper();
        if ((incomingStatus == "COMPLETED" && transaction.Status == TransactionStatus.Completed) ||
        (incomingStatus == "FAILED" && transaction.Status == TransactionStatus.Failed) ||
        (incomingStatus == "INVALID" && transaction.Status == TransactionStatus.Invalid) ||
        (incomingStatus == "REVERSED" && transaction.Status == TransactionStatus.Reversed))
        {
            return true;
        }
        _logger.LogInformation("---------------------------------------------------------------------------[WEBHOOK DEBUG] Pesapal Status is: {incomingStatus}---------------------------------------------------------------------", incomingStatus);
        _logger.LogInformation("[WEBHOOK DEBUG] DB Status BEFORE Update: {transaction.Status}",transaction.Status);
        _logger.LogInformation("[WEBHOOK DEBUG] The tracked Transaction ID is: {transaction.Id}", transaction.Id);
        switch (incomingStatus)
        {
            case "COMPLETED":
                transaction.MarkAsCompleted(confirmationCode, description, paymentMethod);
                break;
            case "FAILED":
                transaction.MarkAsFailed(confirmationCode, description, paymentMethod);
                break;
            case "INVALID":
                transaction.MarkAsInvalid(confirmationCode, description, paymentMethod);
                break;
            case "REVERSED":
                transaction.MarkAsReversed(confirmationCode, description, paymentMethod);
                break;
            default:
                return true; 
        }
        _logger.LogInformation("[WEBHOOK DEBUG] DB Status AFTER Update: {transaction.Status}", transaction.Status);
        _logger.LogInformation("[WEBHOOK DEBUG] Number of Logs attached: {transaction.Logs.Count}", transaction.Logs.Count);

        var rowsAffected = await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[WEBHOOK DEBUG] SaveChanges executed. Rows affected: {rowsAffected}", rowsAffected);

        return true;
    }
}