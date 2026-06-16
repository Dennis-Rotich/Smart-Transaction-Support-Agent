namespace SupportAgent.Contracts;

public class PaymentLinkResponse
{
	public string RedirectUrl { get; set; } = string.Empty;

	public string OrderTrackingId { get; set; } = string.Empty;

	public string? ErrorMessage { get; set; }
}