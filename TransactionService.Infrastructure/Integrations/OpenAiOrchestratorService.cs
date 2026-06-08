using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Tools;

namespace TransactionService.Infrastructure.Integrations;

public class OpenAiOrchestratorService : IAiOrchestratorService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiOrchestratorService> _logger;

    private readonly TransactionTools _transactionTools;
    private readonly SystemTools _systemTools;
    private readonly RetrievalTools _retrievalTools;

    public OpenAiOrchestratorService(IConfiguration configuration, TransactionTools transactionTools, SystemTools systemTools, RetrievalTools retrievalTools, ILogger<OpenAiOrchestratorService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var model = "gpt-4o-mini";

        _chatClient = new ChatClient(model, apiKey);
        _logger = logger;

        _transactionTools = transactionTools;
        _retrievalTools = retrievalTools;
        _systemTools = systemTools;
    }

    public async Task<string> GetChatResponseAsync(string userPrompt)
    {
        _logger.LogInformation("------NEW CHAT REQUEST------");
        _logger.LogInformation("User Prompt: {Prompt}", userPrompt);

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
        var toolClasses = new List<object> { _transactionTools, _systemTools, _retrievalTools };
        var allTools = ToolReflectionEngine.GenerateTools(toolClasses).ToList();

        _logger.LogInformation("Reflection Engine found {Count} total tools.", allTools.Count);

        foreach (var tool in allTools)
        {
            options.Tools.Add(tool);
        }

        bool requiresAction = true;
        int maxIterations = 5; // Guard against infinite loops
        int currentIteration = 0;

        while (requiresAction && currentIteration < maxIterations)
        {
            currentIteration++;
            _logger.LogInformation("Calling OpenAI with tools attached... (Iteration {Index})", currentIteration);

            var completion = await _chatClient.CompleteChatAsync(messages, options);

            // CRITICAL STEP 1: Always add the AI's response (even if it's a tool call request) to the history!
            messages.Add(ChatMessage.CreateAssistantMessage(completion.Value));

            if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                _logger.LogWarning("AI requested tool: {Count} tool calls found.", completion.Value.ToolCalls.Count);

                foreach (var toolCall in completion.Value.ToolCalls)
                {
                    _logger.LogInformation("Executing Tool: {Name} with Args: {Args}",
                        toolCall.FunctionName,
                        toolCall.FunctionArguments.ToString());
                    // Execute your tool using reflection
                    string toolResult = await ToolReflectionEngine.ExecuteToolAsync(
                        toolCall.FunctionName,
                        toolCall.FunctionArguments.ToString(),
                        toolClasses);

                    _logger.LogInformation("Executed '{Name}'. Result length: {Len}", toolCall.FunctionName, toolResult.Length);

                    // CRITICAL STEP 2: Feed the result back, explicitly linked to the ToolCallId
                    messages.Add(ChatMessage.CreateToolMessage(toolCall.Id, toolResult));
                }

                // Keep the loop going so OpenAI can look at the tool results and answer
                requiresAction = true;
            }
            else
            {
                // The AI didn't call a tool; it returned a normal text reply. We are done!
                _logger.LogInformation("AI finished processing. Reason: {Reason}", completion.Value.FinishReason);
                requiresAction = false;
                return completion.Value.Content[0].Text;
            }
        }

        return "The system exceeded maximum tool routing limits.";

    }
}