using OpenAI.Chat;
using TransactionService.Application.DTOs;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class QueryRewriterService : IQueryRewriterService
{
    private readonly global::OpenAI.OpenAIClient _client;

    private const string ModelName = "gpt-4o-mini";

    public QueryRewriterService(global::OpenAI.OpenAIClient client)
    {
        _client = client;
    }

    public async Task<string> RewriteQueryAsync(string currentMessage, List<ChatHistoryDTO> chatHistory, CancellationToken cancellationToken = default)
    {
        ChatClient chatClient = _client.GetChatClient(ModelName);

        var messages = new List<ChatMessage>();

        string systemInstruction = "Read the chat history. Rewrite the user's latest message into a standalone search query. Resolve pronouns (it, they) to their actual subjects. If it's a greeting, return 'NO_SEARCH'.";

        messages.Add(new SystemChatMessage(systemInstruction));

        if(chatHistory != null && chatHistory.Any())
        {
            var recentHistory = chatHistory.TakeLast(5);

            foreach(var msg in recentHistory)
            {
                if(msg.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add(new UserChatMessage(msg.Content));
                }
                else
                {
                    messages.Add(new AssistantChatMessage(msg.Content));
                }
            }
        }

        messages.Add(new UserChatMessage($"Latest Message: {currentMessage}"));
        try
        {
            var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

            return response.Value.Content[0].Text.Trim();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to rewrite query: {ex.Message}", ex);
        }
    }
}