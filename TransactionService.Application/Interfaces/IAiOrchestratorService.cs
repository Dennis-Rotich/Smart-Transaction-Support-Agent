using System.Threading.Tasks;

namespace TransactionService.Application.Interfaces;

public interface IAiOrchestratorService
{
    Task<string> GetChatResponseAsync(string userPrompt);

    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> textChunks, CancellationToken cancellationToken = default);
}