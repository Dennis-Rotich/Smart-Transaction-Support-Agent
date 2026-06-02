using Microsoft.AspNetCore.Mvc;
using MediatR;
using TransactionService.Application.Documents.Commands;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IMediator _mediator;
    public DocumentController(IMediator mediator) => _mediator = mediator;
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded."); 
        }
        using var stream = file.OpenReadStream();
        var command = new UploadDocumentCommand(file.FileName, file.ContentType, stream);
        var response = await _mediator.Send(command);
        return Ok(response);
    }
}