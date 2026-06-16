using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using SupportAgent.Contracts;

namespace SupportAgent.UI.Services;

public class PaymentService 
{
	private readonly HttpClient _httpClient;

	public PaymentService(HttpClient httpClient) 
	{
		_httpClient = httpClient;
	}

	public async Task<PaymentLinkResponse?> GeneratePaymentLinkAsync(string email, decimal amount, string currency)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("http://localhost:63442/api/chat/generate-link", new { Email = email, Currency = currency, Amount = amount });

			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadFromJsonAsync<PaymentLinkResponse>();
			}

			return null;
		}
		catch (Exception)
		{
			return null;
		}
	}
}