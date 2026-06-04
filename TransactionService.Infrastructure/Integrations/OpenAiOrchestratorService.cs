using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Tools;

namespace TransactionService.Infrastructure.Integrations;

public class OpenAiOrchestratorService : IAiOrchestratorService
{
    private readonly ChatClient _chatClient;
    private readonly TransactionTools _transactionTools;
    private readonly SystemTools _systemTools;
    private readonly RetrievalTools _retrievalTools;

    public OpenAiOrchestratorService(IConfiguration configuration, TransactionTools transactionTools, SystemTools systemTools, RetrievalTools retrievalTools)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var model = "gpt-4o-mini";
       
        _chatClient = new ChatClient(model, apiKey);
        _transactionTools = transactionTools;
        _retrievalTools = retrievalTools;
        _systemTools = systemTools;
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

        var options = new ChatCompletionOptions();
        var allToolClasses = new List<object> { _transactionTools, _systemTools, _retrievalTools };

        foreach (var toolClass in allToolClasses)
        {
            foreach (var tool in ToolReflectionEngine.GenerateTools(toolClass))
            {
                options.Tools.Add(tool);
            }
        }

        var requiresAction= true;
        ChatCompletion? completion = null;

        while (requiresAction)
        {
            completion = await _chatClient.CompleteChatAsync(messages, options);

            if (completion.FinishReason == ChatFinishReason.ToolCalls)
            {
                messages.Add(new AssistantChatMessage(completion));

                foreach (var toolCall in completion.ToolCalls)
                {
                    string? toolResultJson = null;

                    foreach (var toolClass in allToolClasses)
                    {
                        toolResultJson = await ToolReflectionEngine.ExecuteToolAsync(
                            toolClass,
                            toolCall.FunctionName,
                            toolCall.FunctionArguments.ToString() ?? "{}"
                        );
                        if (toolResultJson != null) break;
                    }

                    toolResultJson ??= "{\"error\": \"Tool not found in backend.\"}";

                    messages.Add(new ToolChatMessage(toolCall.Id, toolResultJson));
                }
            }
            else
            {
                requiresAction = false; 
            }
        }

        return completion?.Content[0].Text ?? "Error: No response generated.";

    }

}