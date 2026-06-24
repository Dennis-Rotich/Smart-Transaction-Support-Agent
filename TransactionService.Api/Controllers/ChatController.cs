using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Commands;
using MediatR;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAiOrchestratorService _orchestratorService;
    private readonly IMediator _mediator; 

    public ChatController(IAiOrchestratorService orchestratorService, IMediator mediator)
    {
        _orchestratorService = orchestratorService;
        _mediator = mediator;
    }

    [HttpPost("test/prompt")]
    public async Task<IActionResult> TestPrompt([FromBody] ChatRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { error = "Prompt cannot be empty." });
        }

        try
        {   
            var recentHistory = request.History != null ? request.History.TakeLast(5).ToList() 
                : new List<ChatMessageDto>();

            string aiResponse = await _orchestratorService.GetChatResponseAsync(request.Prompt, recentHistory);

            return Ok(new { response = aiResponse });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request.", detail = ex.Message });
        }
    }

    [HttpPost("generate-link")]
    public async Task<IActionResult> GenerateLink([FromBody] GenerateLinkCommand command)
    {
        if(string.IsNullOrWhiteSpace(command.Email)) return BadRequest("Email is required");
        if(string.IsNullOrWhiteSpace(command.Currency)) return BadRequest("Currency is required.");
        if(command.Amount == 0) return BadRequest("Amount must be greater than zero.");

        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request.", detail = ex.Message });
        }
    }

}
public class ChatRequest
{
    public string Prompt { get; set; } = string.Empty;
    public List<ChatMessageDto> History { get; set; } = new();
}
