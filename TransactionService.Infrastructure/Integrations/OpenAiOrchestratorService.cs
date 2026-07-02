using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TransactionService.Application.Interfaces;
using TransactionService.Infrastructure.Tools;
using System.ClientModel;

namespace TransactionService.Infrastructure.Integrations;

public class OpenAiOrchestratorService : IAiOrchestratorService
{
    private readonly ChatClient _chatClient;
    private readonly global::OpenAI.OpenAIClient _openAIClient;
    private readonly ILogger<OpenAiOrchestratorService> _logger;

    private readonly IServiceProvider _serviceProvider;

    private const string EmbeddingModel = "text-embedding-3-small";

    public OpenAiOrchestratorService(OpenAI.OpenAIClient openAIClient,IConfiguration configuration,IServiceProvider serviceProvider, ILogger<OpenAiOrchestratorService> logger)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        var model = "gpt-4o-mini";

        _chatClient = new ChatClient(model, apiKey);
        _openAIClient = openAIClient;
        _logger = logger;

        _serviceProvider = serviceProvider;
    }

    public async Task<string> GetChatResponseAsync(string userPrompt, List<ChatMessageDto> history)
    {
        _logger.LogInformation("------NEW CHAT REQUEST------");
        _logger.LogInformation("User Prompt: {Prompt}", userPrompt);

        var systemInstruction = """
            You are an expert Pesapal API Integration Assistant. 
            Your SOLE purpose is to help developers integrate our APIs by referencing official documentation.

            CORE DIRECTIVES:
            1. Search First: You must ALWAYS use the SearchApiDocumentation tool to find answers to any user query.
            2. Strict Adherence: Base your technical advice STRICTLY on the text returned by the tool. 
            3. Anti-Hallucination: If the SearchApiDocumentation tool returns no results, or if the retrieved text does not contain the answer, you must admit that it's not in the documentation. NEVER guess, assume, or rely on your pre-training data.
            
            FORMATTING RULES:
            - Structure your answers using clean Markdown.
            - Format code snippets, endpoints, and JSON payloads within Markdown code blocks.
            """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemInstruction),
        };

        if(history != null && history.Any())
        {
            foreach(var msg in history)
            {
                if(msg.Role.Equals("User", StringComparison.OrdinalIgnoreCase))
                {
                    messages.Add(new UserChatMessage(msg.Content));
                }
                else
                {
                    messages.Add(new SystemChatMessage(msg.Content));
                }
            }
        }

        messages.Add(new UserChatMessage(userPrompt));

        //var transactionTools = _serviceProvider.GetRequiredService<TransactionTools>();
        //var systemTools = _serviceProvider.GetRequiredService<SystemTools>();
        var retrievalTools = _serviceProvider.GetRequiredService<RetrievalTools>();

        var options = new ChatCompletionOptions();
        var toolClasses = new List<object> { retrievalTools };
        var allTools = ToolReflectionEngine.GenerateTools(toolClasses).ToList();

        _logger.LogInformation("Reflection Engine found {Count} total tools.", allTools.Count);

        foreach (var tool in allTools)
        {
            options.Tools.Add(tool);
        }

        bool requiresAction = true;
        int maxIterations = 5;
        int currentIteration = 0;

        while (requiresAction && currentIteration < maxIterations)
        {
            currentIteration++;
            _logger.LogInformation("Calling OpenAI with tools attached... (Iteration {Index})", currentIteration);

            var completion = await _chatClient.CompleteChatAsync(messages, options);

            messages.Add(ChatMessage.CreateAssistantMessage(completion.Value));

            if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
            {
                _logger.LogWarning("AI requested tool: {Count} tool calls found.", completion.Value.ToolCalls.Count);

                foreach (var toolCall in completion.Value.ToolCalls)
                {
                    _logger.LogInformation("Executing Tool: {Name} with Args: {Args}",
                        toolCall.FunctionName,
                        toolCall.FunctionArguments.ToString());

                    string toolResult = await ToolReflectionEngine.ExecuteToolAsync(
                        toolCall.FunctionName,
                        toolCall.FunctionArguments.ToString(),
                        toolClasses, _logger);

                    _logger.LogInformation("Executed '{Name}'. Result length: {Len}", toolCall.FunctionName, toolResult.Length);

                    messages.Add(ChatMessage.CreateToolMessage(toolCall.Id, toolResult));
                }

                requiresAction = true;
            }
            else
            {
                _logger.LogInformation("AI finished processing. Reason: {Reason}", completion.Value.FinishReason);
                requiresAction = false;
                return completion.Value.Content[0].Text;
            }
        }

        return "The system exceeded maximum tool routing limits.";

    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> textChunks, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEBUG] GenerateEmbeddingsAsync called with {Count} chunks.", textChunks?.Count ?? 0);

        if (textChunks != null && textChunks.Any())
        {
            _logger.LogInformation("[DEBUG] Chunk 0 length: {Len}, content: '{Content}'", textChunks[0].Length, textChunks[0]);
        }

        var embeddings = new List<float[]>();

        try
        {
            EmbeddingClient embeddingClient = _openAIClient.GetEmbeddingClient(EmbeddingModel);

            var response = await embeddingClient.GenerateEmbeddingsAsync(textChunks, cancellationToken: cancellationToken);

            var responseCount = response.Value?.Count() ?? 0;
            _logger.LogInformation("[DEBUG] OpenAI API returned {Count} embedding values.", responseCount);

            if(response.Value != null)
            {
                foreach (var item in response.Value)
                {
                    embeddings.Add(item.ToFloats().ToArray());
                }
            }

            _logger.LogInformation("[DEBUG] Returning {Count} mapped vectors back to caller.", embeddings.Count);

            return embeddings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DEBUG] Exception thrown inside GenerateEmbeddingsAsync");
            throw new ApplicationException($"Failed to generate embeddings from OpenAI: {ex.Message}", ex);
        }
    }
}