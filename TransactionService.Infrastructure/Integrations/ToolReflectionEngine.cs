using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI.Chat;
using ModelContextProtocol.Server;

namespace TransactionService.Infrastructure.Integrations;

public static class ToolReflectionEngine
{
    public static IEnumerable<ChatTool> GenerateTools(object toolClassInstance)
    {
        var tools = new List<ChatTool>();

        var methods = toolClassInstance.GetType().GetMethods()
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

        foreach (var method in methods){
            var mcpAttr = method.GetCustomAttribute<McpServerToolAttribute>();
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();

            var description = descAttr?.Description ?? "No description provided.";
            var parameters = method.GetParameters();


            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            foreach (var param in parameters)
            {
                properties.Add(param.Name!, new { type = "string", description = $"Parameter: {param.Name}" });
                required.Add(param.Name!);
            }

            var functionJson = new
            {
                type = "object",
                properties = properties,
                required = required
            };

            tools.Add(ChatTool.CreateFunctionTool(
                 mcpAttr!.Name ?? method.Name,
                 description,
                 BinaryData.FromObjectAsJson(functionJson)
            ));
        }

        return tools;
    }

    public static async Task<string> ExecuteToolAsync(object toolClassInstance, string toolName, string jsonArguments)
    {
        var method = toolClassInstance.GetType().GetMethods()
            .FirstOrDefault(m => m.GetCustomAttribute<McpServerToolAttribute>()?.Name == toolName || m.Name == toolName);

        if(method == null) return $"Tool '{toolName}' not found.";

        using var doc = JsonDocument.Parse(jsonArguments);
        var parameters = method.GetParameters();
        var args = new object[parameters.Length];

        for(int i = 0; i < parameters.Length; i++)
        {
            var propName = parameters[i].Name;
            if(doc.RootElement.TryGetProperty(propName!,out var element))
            {
                args[i] = element.GetString()!;
            }
        }

        if(method.ReturnType == typeof(Task<string>))
        {
            return await (Task<string>)method.Invoke(toolClassInstance, args)!;
        }

        return "{\"error\": \"Tool execution failed due to unsupported return type.\"}";
    }
}