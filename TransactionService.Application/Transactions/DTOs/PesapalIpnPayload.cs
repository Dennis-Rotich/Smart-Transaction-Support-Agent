namespace TransactionService.Application.Transactions.DTOs;

public class PesapalIpnPayload
{
    public string? OrderTrackingId { get; set; }
    public string? OrderNotificationType { get; set; }
    public string? OrderMerchantReference { get; set; }
}