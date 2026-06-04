using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace SupportAgent.UI.Services;

public class McpClientService
{
    private readonly HttpClient _httpClient;
    public McpClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<string> SendPromptAsync(string userInput)
    {
        try
        {
            var payload = new { prompt = userInput };
            var jsonString = JsonSerializer.Serialize(payload);

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PostAsync("http://localhost:63442/api/chat/test/prompt", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                return $"[Orchestrator Error: {response.StatusCode}] {errorDetail}";
            }

            var rawResponse = await response.Content.ReadAsStringAsync();

            try
            {
                using var jsonDoc = JsonDocument.Parse(rawResponse);

                if (jsonDoc.RootElement.TryGetProperty("response", out var responseElement))
                {
                    return responseElement.GetString() ?? "No content returned.";
                }
                if (jsonDoc.RootElement.TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? "No content returned.";
                }

                return rawResponse;
            }
            catch (JsonException)
            {
                return rawResponse;
            }
        }
        catch (Exception ex)
        {
            return $"[Connection Error: Failed to reach Orchestrator API. {ex.Message}]";
        }
    }

    
}