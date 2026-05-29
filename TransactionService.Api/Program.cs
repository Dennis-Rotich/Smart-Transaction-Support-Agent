using TransactionService.Infrastructure;
using TransactionService.Application.Transactions.Commands;
using ModelContextProtocol.Server;
using TransactionService.Api.Tools;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

builder.Services.AddTransient<SystemTools>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true; 
    })
    .WithToolsFromAssembly(typeof(SystemTools).Assembly);

builder.Services.AddTransient<TransactionTools>();

builder.Services.AddTransient<RetrievalTools>();

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly(typeof(TransactionTools).Assembly);

var app = builder.Build();

// app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp");

app.Run();