using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Transactions.Commands;
using TransactionService.Application.Transactions.Queries;

namespace TransactionService.Infrastructure.Tools;

[McpServerToolType]
public class TransactionTools
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public TransactionTools(IMediator mediator, ILogger<TransactionTools> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [McpServerTool]
    [Description("Creates a new Pesapal payment order. Requires customer name, email, amount, and a unique reference. Returns the generated tracking ID and payment link.")]
    public async Task<string> CreatePaymentOrder([Description("The currency for the transaction in three capitalized words, e.g, KES")]string currency, [Description("The exact amount to be charged")]decimal amount)
    {
        var command = new CreateTransactionCommand(amount, currency);

        var result = await _mediator.Send(command);

        return $"Payment Order Created Successfully.\nReference: {result.Reference}\nPayment URL: {result.PaymentUrl}";
    }

    [McpServerTool]
    [Description("Checks the current payment status of a transaction using the reference.")]
    public async Task<string> CheckTransactionStatus([Description("The reference of the transaction e.g, TXN-A0078DD3")]string reference)
    {
        var query = new GetTransactionStatusQuery(reference);
        var result = await _mediator.Send(query);
        if(result == null)
        {   
            _logger.LogWarning("No transaction found matching reference '{Reference}'.", reference);
            return $"Error: No transaction found matching reference '{reference}'.";
        }
        return $"Transaction Status for '{reference}': {result.Status}";
    }

    [McpServerTool]
    [Description("Retrieves the full, detailed information for a specific transaction using its reference ID.")]
    public async Task<string> GetTransactionDetails([Description("The reference of the transaction e.g, TXN-A0078DD3")]string reference)
    {
        var query = new GetTransactionDetailsQuery(reference);
        var result = await _mediator.Send(query);

        if(result == null)
        {   
            _logger.LogWarning("No transaction details found for reference '{Reference}'.", reference);
            return $"Error: No transaction details found for reference '{reference}'.";
        }

        var detailsBlock = $@"Transaction Details for '{reference}':
            - Reference: {result.Reference}
            - Amount: {result.Amount}
            - Currency: {result.Currency}
            - Date Created: {result.CreatedAt:g}
            - Current Status: {result.Status}
            - Provider Tracking ID: {result.ExternalTrackingId}";

        var logsBlock = "\n\nTransaction History Logs:\n";

        if (result.Logs != null && result.Logs.Any())
        {
            // We use LINQ and string.Join to create a clean text list for the LLM to read
            var formattedLogs = string.Join("\n", result.Logs.Select(log =>
                $"  -> {log.Message} | Response Code: {log.ProviderResponseCode ?? "None"} | Response Body: {log.ProviderResponseBody ?? "None"}" 
            ));
            logsBlock += formattedLogs;
        }
        else
        {
            logsBlock += "  -> No historical logs found for this transaction.";
        }

        return detailsBlock + logsBlock;
    }

    [McpServerTool]
    [Description("Retrieves a list of the most recently created payment transactions. Useful for summaries or finding recent activity.")]
    public async Task<string> ListRecentTransactions()
    {
        var query = new ListRecentTransactionsQuery(10);
        var result = await _mediator.Send(query);

        if(result == null || !result.Any())
        {   
            _logger.LogInformation("No recent transactions found.");
            return "No recent transactions found.";
        }

        var formattedList = string.Join("\n", result.Select(r => $"- Ref: {r.Reference} | Amount: {r.Amount} | Status: {r.Status} | Date: {r.CreatedAt:g}"));

        return formattedList;
    }
}
