using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Transactions.Commands;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreatePaymentOrder([FromBody] CreateTransactionCommand command)
    {
        if (command.Amount <= 0) return BadRequest("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(command.Currency)) return BadRequest("Currency is required.");

        var response = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetTransactionStatus), new { reference = response.Reference }, response);
    }

    [HttpGet("{reference}/status")]
    public async Task<IActionResult> GetTransactionStatus(string reference)
    {
        var query = new GetTransactionStatusQuery(reference);
        var response = await _mediator.Send(query);

        if (response == null) return NotFound($"Transaction with reference {reference} was not found.");

        return Ok(response);
    }

    [HttpPost("setup-ipn")]
    public async Task<IActionResult> SetupIpn([FromServices] IPaymentGatewayService gateway)
    {
        var token = await gateway.GetTokenAsync();

        var ipnId = await gateway.RegisterIpnAsync(token, "https://uncruel-autogamous-kacie.ngrok-free.dev/api/transaction/ipn");

        return Ok(new { Message = "Copy this ID into your appsettings.json!", IpnId = ipnId });
    }

    [HttpPost("ipn")]
    public async Task<IActionResult> ReceiveIpn([FromQuery] string OrderTrackingId, [FromQuery] string IpnNotificationId)
    {
        var command = new ProcessIpnCommand(OrderTrackingId, IpnNotificationId);
        var result = await _mediator.Send(command);

        return Ok(new
        {
            OrderTrackingId = OrderTrackingId,
            Status = "OK"
        });
    }
}