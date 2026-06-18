using TransactionService.Infrastructure;
using TransactionService.Application.Transactions.Commands;
using ModelContextProtocol.Server;
using TransactionService.Infrastructure.Tools;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Configurations;
using TransactionService.Infrastructure.Persistence.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/eldo-agent-.txt", rollingInterval: RollingInterval.Day) 
    .CreateLogger();

builder.Host.UseSerilog();

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

builder.Services.Configure<PineconeOptions>(
    builder.Configuration.GetSection("Pinecone"));

var app = builder.Build();

//app.UseHttpsRedirection();
app.UseAuthorization();

app.UseCors("AllowBlazorUI");

app.MapControllers();
app.MapMcp("/mcp");

app.Run();