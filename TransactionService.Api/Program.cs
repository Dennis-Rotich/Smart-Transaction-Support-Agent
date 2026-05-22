using TransactionService.Infrastructure;
using TransactionService.Application.Transactions.Commands;
using ModelContextProtocol.Server;
using TransactionService.Api.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddTransient<SystemTools>();

builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true; 
    })
    .WithToolsFromAssembly(typeof(SystemTools).Assembly);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapMcp("/mcp");

app.Run();