using System.Threading.Tasks;

namespace TransactionService.Application.Interfaces;

public interface IAiOrchestratorService
{
    Task<string> GetChatResponseAsync(string userPrompt);
}