using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace SupportAgent.UI.Services;

public class UploadDocumentService
{
    private readonly HttpClient _httpClient;
    public UploadDocumentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> UploadDocumentAsync(IBrowserFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            var maxAllowedSize = 1024 * 1024 * 5;
            using var fileStream = file.OpenReadStream(maxAllowedSize);

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var streamContent = new StreamContent(memoryStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync("http://localhost:63442/api/document/upload", content);

            if (response.IsSuccessStatusCode)
            {
                return $"Successfully uploaded {file.Name}.";
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Upload failed with status {response.StatusCode}: {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload exception: {ex.Message}");
            return null;
        }
    }
}