using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> GetTokenAsync();

    Task<string> RegisterIpnAsync(string token, string webhookUrl);

    Task<(string RedirectUrl, string OrderTrackingId)> SubmitOrderAsync(string token, decimal amount, string currency, string reference, string email);

    Task<PesapalStatusResponse> GetTransactionStatusAsync(string token, string orderTrackingId);
}