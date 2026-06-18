using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Repositories;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Integrations;
using Pinecone;
using Microsoft.Extensions.Options;
using TransactionService.Application.Configurations;

namespace TransactionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")),
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
        services.AddScoped<IAiOrchestratorService, OpenAiOrchestratorService>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IPaymentGatewayService, PesapalGatewayService>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PineconeOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new InvalidOperationException("Pinecone Api Key is missing");
            }

            var loggerFactory = sp.GetService<ILoggerFactory>();

            return new PineconeClient(options.ApiKey, loggerFactory);
        });

        return services;
    }
}