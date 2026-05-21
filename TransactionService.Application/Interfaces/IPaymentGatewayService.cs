namespace TransactionService.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<string> GetTokenAsync();

    Task<string> RegisterIpnAsync(string token, string webhookUrl);

    Task<(string RedirectUrl, string OrderTrackingId)> SubmitOrderAsync(string token, decimal amount, string currency, string reference);

    Task<(string Status, string ProviderReference)> GetTransactionStatusAsync(string token, string orderTrackingId);
}