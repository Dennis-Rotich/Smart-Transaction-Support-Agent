using TransactionService.Infrastructure;
using TransactionService.Application.Transactions.Commands;
using ModelContextProtocol.Server;
using TransactionService.Infrastructure.Tools;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Configurations;
using TransactionService.Infrastructure.Persistence.Repositories;
using Serilog;
using Qdrant.Client;
using Qdrant.Client.Grpc;

var builder = WebApplication.CreateBuilder(args);

string logDirectory = Path.Combine(AppContext.BaseDirectory, "logs", "eldo-agent-.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(logDirectory, rollingInterval: RollingInterval.Day) 
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

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly(typeof(TransactionTools).Assembly);

builder.Services.Configure<PineconeOptions>(
    builder.Configuration.GetSection("Pinecone"));

var app = builder.Build();

try
{
    var qdrantClient = new QdrantClient("localhost", 6334);
    var collectionName = "api-docs";

    var collectionExists = await qdrantClient.CollectionExistsAsync(collectionName);

    if (!collectionExists)
    {
        await qdrantClient.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: new VectorParams
            {
                Size = 1536,
                Distance = Distance.Cosine
            }
            );
    }
    Console.WriteLine($"[Qdrant] Successfully initialized '{collectionName}' collection.");
}
catch (Exception ex)
{
    Console.WriteLine($"[Qdrant] Warning: Could not connect to local Qdrant instance. Did you start the Docker container? Error: {ex.Message}");
}

//app.UseHttpsRedirection();
app.UseAuthorization();

app.UseCors("AllowBlazorUI");

app.MapControllers();
app.MapMcp("/mcp");

app.Run();