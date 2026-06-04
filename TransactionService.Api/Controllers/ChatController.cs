using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TransactionService.Application.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IAiOrchestratorService _orchestratorService;

    public ChatController(IAiOrchestratorService orchestratorService)
    {
        _orchestratorService = orchestratorService;
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
            string aiResponse = await _orchestratorService.GetChatResponseAsync(request.Prompt);

            return Ok(new { response = aiResponse });
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request.", detail = ex.Message });
        }
    }

}
public class ChatRequest
{
    public string Prompt { get; set; } = string.Empty;
}
