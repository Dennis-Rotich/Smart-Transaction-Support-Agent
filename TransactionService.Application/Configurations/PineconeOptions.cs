namespace TransactionService.Application.Configurations;

public class PineconeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string HostUrl { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
}