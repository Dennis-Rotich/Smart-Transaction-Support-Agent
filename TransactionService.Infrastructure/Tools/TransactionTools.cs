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
    [Description("Retrieves the full, detailed information for a specific transaction using its merchant reference ")]
    public async Task<string> GetTransactionDetailsByMerchantReference([Description("The merchant reference of the transaction e.g, TXN-A0078DD3")]string reference)
    {
        var query = new GetTransactionDetailsByMerchantReferenceQuery(reference);
        var result = await _mediator.Send(query);

        if(result == null)
        {   
            _logger.LogWarning("No transaction details found for reference '{Reference}'.", reference);
            return $"Error: No transaction details found for reference '{reference}'.";
        }

        var detailsBlock = $@"Transaction Details for '{reference}':
            - Merchant Reference: {result.MerchantReference}
            - Transaction Reference: {result.TransactionReference}
            - Payment Method: {result.PaymentMethod}
            - Amount: {result.Amount}
            - Currency: {result.Currency}
            - Date Created: {result.CreatedAt:g}
            - Current Status: {result.Status}
            - Provider Tracking ID: {result.OrderTrackingId}";

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
    [Description("Retrieves the full, detailed information for a specific transaction using its transaction reference or confirmation code ")]
    public async Task<string> GetTransactionDetailsByTransactionReference([Description("The transaction reference or confirmation code of the transaction e.g, 7812558239596369704604")] string reference)
    {
        var query = new GetTransactionDetailsByTransactionReferenceQuery(reference);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("No transaction details found for reference '{Reference}'.", reference);
            return $"Error: No transaction details found for reference '{reference}'.";
        }

        var detailsBlock = $@"Transaction Details for '{reference}':
            - Merchant Reference: {result.MerchantReference}
            - Transaction Reference: {result.TransactionReference}
            - Payment Method: {result.PaymentMethod}
            - Amount: {result.Amount}
            - Currency: {result.Currency}
            - Date Created: {result.CreatedAt:g}
            - Current Status: {result.Status}
            - Provider Tracking ID: {result.OrderTrackingId}";

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
    [Description("Retrieves the full, detailed information for a specific transaction using its order tracking id ")]
    public async Task<string> GetTransactionDetailsByTrackingId([Description("The order tracking id of the transaction e.g, e7e44bac-04ef-408b-b3c1-da4c354faeed")] string trackingId)
    {
        var query = new GetTransactionDetailsByTrackingIdQuery(trackingId);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("No transaction details found for tracking id '{TrackingId}'.", trackingId);
            return $"Error: No transaction details found for trackingId '{trackingId}'.";
        }

        var detailsBlock = $@"Transaction Details for '{trackingId}':
            - Merchant Reference: {result.MerchantReference}
            - Transaction Reference: {result.TransactionReference}
            - Payment Method: {result.PaymentMethod}
            - Amount: {result.Amount}
            - Currency: {result.Currency}
            - Date Created: {result.CreatedAt:g}
            - Current Status: {result.Status}
            - Provider Tracking ID: {result.OrderTrackingId}";

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

        var formattedList = string.Join("\n", result.Select(r => $"- Merchant Ref: {r.MerchantReference} | Transaction Ref: {r.TransactionReference} | Payment Method: {r.PaymentMethod} | Tracking ID: {r.OrderTrackingId} | Amount: {r.Amount} | Status: {r.Status} | Date: {r.CreatedAt:g}"));

        return formattedList;
    }
}
