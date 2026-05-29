using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

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
            var payload = new
            {
                jsonrpc = "2.0",
                id = Guid.NewGuid().ToString(),
                method = "tools/call",
                @params = new Dictionary<string, object>
                    {
                        { "name", "create_payment_order" }, // Targeting the transaction tool!
                        { "arguments", new {
                            amount = 1500.00,
                            currency = "KES",
                            customerEmail = "test@domain.com",
                            reference = "INV-" + Guid.NewGuid().ToString().Substring(0, 8)
                        } }
                    }
            };

            var jsonString = JsonSerializer.Serialize(payload);

            
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var content = new ByteArrayContent(jsonBytes);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("*/*"));

            var response = await _httpClient.PostAsync("http://localhost:63442/mcp", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                return $"[System Error: {response.StatusCode}] {errorDetail}";
            }

            var rawResponse = await response.Content.ReadAsStringAsync();

            string jsonToParse = rawResponse;

            if (rawResponse.Contains("data: "))
            {
                var parts = rawResponse.Split("data: ");
                if (parts.Length > 1)
                {
                    jsonToParse = parts[1].Trim();
                }
            }
            
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonToParse);

                if (jsonDoc.RootElement.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("content", out var contentArray) &&
                    contentArray.GetArrayLength() > 0 &&
                    contentArray[0].TryGetProperty("text", out var textElement))
                {
                    return textElement.GetString() ?? "No results found.";
                }

                return $"[Valid JSON, Wrong Structure]: {jsonToParse}";
            }
            catch (JsonException)
            {
                return $"[Raw Backend Text]: {rawResponse}";
            }
        }
         catch (Exception ex) 
        {
            return $"[System Error: Failed to connect to backend. {ex.Message}]";
        }
    }
}