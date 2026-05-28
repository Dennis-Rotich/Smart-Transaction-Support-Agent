using System;

namespace TransactionService.Domain.Entities;

public class KnowledgeArticle : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}