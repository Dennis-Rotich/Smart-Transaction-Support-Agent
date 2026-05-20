using MediatR;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Application.Transactions.Commands;
using TransactionService.Application.Transactions.Queries;

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
}