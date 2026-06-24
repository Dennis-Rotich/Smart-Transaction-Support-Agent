using System.Threading.Tasks;

namespace TransactionService.Application.Interfaces;

public interface IAiOrchestratorService
{
    Task<string> GetChatResponseAsync(string userPrompt, List<ChatMessageDto> history);

    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> textChunks, CancellationToken cancellationToken = default);
}

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}