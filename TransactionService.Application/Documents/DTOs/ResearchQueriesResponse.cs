
namespace TransactionService.Application.Documents.DTOs;

// Log search
public record LogResultDto(DateTime Timestamp, string Level, string Message, string Source);


// Document retrieval
public record DocumentResultDto(string Title, string Version, string Content);


// Knowledge search
public record KnowledgeResultDto(string Title, string Excerpt, string Content, double Score);
