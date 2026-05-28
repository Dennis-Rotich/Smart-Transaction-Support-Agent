namespace SupportAgent.UI.Models;

public class ChatMessage
{
    public string Role { get; set; } = String.Empty;
    public string Content { get; set; } = String.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}