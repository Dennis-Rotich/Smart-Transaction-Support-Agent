using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class OpenAiOrchestratorService : IAiOrchestratorService
{
    private readonly ChatClient _chatClient;

    public OpenAiOrchestratorService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var model = configuration["OpenAI:Model"] ?? "gpt-40-mini";
       
        _chatClient = new ChatClient(model, apiKey);
    }

    public async Task<string> GetChatResponseAsync(string userPrompt)
    {
        var systemInstruction = """
            You are 'Eldo', an expert IT support and transaction analysis agent.
            Your primary role is to assist users with payment processing, log analysis, and system documentation.

            CORE DIRECTIVES:
            1. Tool Usage: Always attempt to use your provided tools to look up real-time data before answering. 
            2. No Hallucination: NEVER invent transaction IDs, payment statuses, or log entries. If you cannot find the data via your tools, state clearly that the record does not exist.
            3. Action Confirmation: Before executing a tool that creates a new record (like creating a payment order), confirm the details with the user if anything is missing.

            FORMATTING RULES:
            - Use Markdown to structure your responses.
            - Present lists of transactions or logs in clean Markdown tables.
            - Highlight critical data like `Transaction IDs` or `Reference Codes` in inline code blocks.
            - Keep your tone concise, professional, and strictly focused on technical and transactional support.
            """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemInstruction),
            new UserChatMessage(userPrompt)
        };

        var completion = await _chatClient.CompleteChatAsync(messages);

        return completion.Value.Content[0].Text;

    }
}