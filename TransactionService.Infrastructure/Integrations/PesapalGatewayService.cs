using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class PesapalGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PesapalGatewayService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(_configuration["Pesapal:BaseUrl"]!);
    }

    public async Task<string> GetTokenAsync()
    {
        var payload = new
        {
            consumer_key = _configuration["Pesapal:ConsumerKey"],
            consumer_secret = _configuration["Pesapal:ConsumerSecret"]
        };

        var response = await _httpClient.PostAsJsonAsync("api/Auth/RequestToken", payload);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);

        return jsonDoc.RootElement.GetProperty("token").GetString()!;
    }

    public async Task<string> RegisterIpnAsync(string token, string webhookUrl)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            url = webhookUrl,
            ipn_notification_type = "POST"
        };

        var response = await _httpClient.PostAsJsonAsync("api/URLSetup/RegisterIPN", payload);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);

        return jsonDoc.RootElement.GetProperty("ipn_id").GetString()!;
    }

    public async Task<(string RedirectUrl, string OrderTrackingId)> SubmitOrderAsync(string token, decimal amount, string currency, string reference)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            id = reference,
            currency = currency,
            amount = amount,
            description = "Payment for Order " + reference,
            callback_url = "https://yourfrontend.com/payment-success", 
            notification_id = _configuration["Pesapal:IpnId"], 
            billing_address = new
            {
                email_address = "customer@example.com", 
                phone_number = "0110931140",
                country_code = "KE",
                first_name = "John",
                last_name = "Doe"
            }
        };

        var response = await _httpClient.PostAsJsonAsync("api/Transactions/SubmitOrderRequest", payload);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        string redirectUrl = root.GetProperty("redirect_url").GetString()!;
        string orderTrackingId = root.GetProperty("order_tracking_id").GetString()!;

        return (redirectUrl, orderTrackingId);
    }

    public async Task<(string Status, string ProviderReference)> GetTransactionStatusAsync(string token, string orderTrackingId)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync($"api/Transactions/GetTransactionStatus?orderTrackingId={orderTrackingId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(content);
        var root = jsonDoc.RootElement;

        string status = root.GetProperty("payment_status_description").GetString()!;
        string providerReference = root.GetProperty("payment_account").GetString()!;

        return (status, providerReference);
    }
}