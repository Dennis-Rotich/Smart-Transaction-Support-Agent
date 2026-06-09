using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI.Chat;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

namespace TransactionService.Infrastructure.Integrations;

public static class ToolReflectionEngine
{
    public static IEnumerable<ChatTool> GenerateTools(IEnumerable<object> toolClassInstances)
    {
        var tools = new List<ChatTool>();

        foreach (var instance in toolClassInstances)
        {
            var methods = instance.GetType().GetMethods()
                .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null);

            foreach (var method in methods)
            {
                var mcpAttr = method.GetCustomAttribute<McpServerToolAttribute>();
                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                var description = descAttr?.Description ?? "No description provided.";
                var parameters = method.GetParameters();

                var properties = new Dictionary<string, object>();
                var required = new List<string>();

                foreach (var param in parameters)
                {
                    var paramDescAttr = param.GetCustomAttribute<DescriptionAttribute>();
                    var paramDescription = paramDescAttr?.Description ?? $"Parameter: {param.Name}";
                    var jsonType = MapCSharpTypeToJsonType(param.ParameterType);

                    properties.Add(param.Name!, new { type = jsonType, description = paramDescription });

                    if (!param.HasDefaultValue)
                    {
                        required.Add(param.Name!);
                    }
                }

                var functionJson = new { type = "object", properties = properties, required = required };

                tools.Add(ChatTool.CreateFunctionTool(
                     mcpAttr!.Name ?? method.Name,
                     description,
                     BinaryData.FromObjectAsJson(functionJson)
                ));
            }
        }

        return tools;
    }

    public static async Task<string> ExecuteToolAsync(string functionName, string argumentsJson, IEnumerable<object> toolClasses, ILogger logger)
    {
        try
        {
            foreach (var instance in toolClasses)
            {
                var method = instance.GetType().GetMethods()
                    .FirstOrDefault(m =>
                        m.GetCustomAttribute<McpServerToolAttribute>()?.Name == functionName ||
                        m.Name == functionName);

                if (method != null)
                {
                    var argsDict = string.IsNullOrWhiteSpace(argumentsJson)
                        ? new Dictionary<string, JsonElement>()
                        : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(argumentsJson);

                    var parameters = method.GetParameters();
                    var invokeArgs = new object?[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        if (argsDict != null && argsDict.TryGetValue(param.Name!, out var jsonElement))
                        {
                            invokeArgs[i] = jsonElement.Deserialize(param.ParameterType);
                        }
                        else if (param.HasDefaultValue)
                        {
                            invokeArgs[i] = param.DefaultValue;
                        }
                        else
                        {
                            invokeArgs[i] = null;
                        }
                    }

                    var result = method.Invoke(instance, invokeArgs);

                    if (result is Task task)
                    {
                        await task;

                        var taskType = task.GetType();
                        if (taskType.IsGenericType)
                        {
                            var resultProperty = taskType.GetProperty("Result");
                            result = resultProperty?.GetValue(task);
                        }
                        else
                        {
                            return "Success";
                        }
                    }

                    if (result is string stringResult) return stringResult;
                    return JsonSerializer.Serialize(result);
                }
            }

            logger.LogWarning("Tool not found: {FunctionName} with args: {Arguments}", functionName, argumentsJson);
            return $"{{\"error\": \"Tool '{functionName}' not found in backend.\"}}";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing tool: {FunctionName} with args: {Arguments}", functionName, argumentsJson);
            return $"{{\"error\": \"Backend execution failed: {ex.InnerException?.Message ?? ex.Message}\"}}";
        }
    }

    public static string MapCSharpTypeToJsonType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if(underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short))
        {
            return "integer";
        }

        if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
        {
            return "number";
        }

        if (underlyingType == typeof(bool))
        {
            return "boolean";
        }

        if(underlyingType.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType) && underlyingType != typeof(string))
        {
            return "array";
        }

        return "string";
    }
}