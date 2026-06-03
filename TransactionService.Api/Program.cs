using TransactionService.Infrastructure;
using TransactionService.Application.Transactions.Commands;
using ModelContextProtocol.Server;
using TransactionService.Api.Tools;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorUI", policy =>
    {
        policy.WithOrigins("https://localhost:5159", "http://localhost:4000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

builder.Services.AddTransient<SystemTools>();
builder.Services.AddTransient<TransactionTools>();
builder.Services.AddTransient<RetrievalTools>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly(typeof(TransactionTools).Assembly);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseCors("AllowBlazorUI");

app.MapControllers();
app.MapMcp("/mcp");

// TO BE DELETED: Temporary endpoint for testing the AI orchestrator service
app.MapPost("/api/chat/test", async (string prompt, IAiOrchestratorService aiService) =>
{
    if(string.IsNullOrWhiteSpace(prompt)) return Results.BadRequest("Prompt cannot be empty.");

    var response = await aiService.GetChatResponseAsync(prompt);
    return Results.Ok(new {Response = response});
});

app.Run();