using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly ApplicationDbContext _context;
    public KnowledgeBaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<KnowledgeResultDto>> SearchKnowledgeAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<KnowledgeResultDto>();

        var lowerQuery = query.ToLower();
        var results = await _context.KnowledgeArticles
            .Where(k => (k.Title != null && k.Title.ToLower().Contains(lowerQuery)) ||
                        (k.Excerpt != null && k.Excerpt.ToLower().Contains(lowerQuery)) ||
                        (k.Content != null && k.Content.ToLower().Contains(lowerQuery)))
            .Take(5)
            .ToListAsync();
        
        return results.Select(k => {
            double simulatedScore = k.Title != null && k.Title.ToLower().Contains(lowerQuery) ? 0.9 :
                                    (k.Excerpt != null && k.Excerpt.ToLower().Contains(lowerQuery)) ? 0.7 :
                                    (k.Content != null && k.Content.ToLower().Contains(lowerQuery)) ? 0.5 : 0.0;

            return new KnowledgeResultDto(
                    k.Title ?? "Untitled Article",
                    k.Excerpt ?? "No excerpt available.",
                    k.Content ?? "No content available.",
                    simulatedScore
                );
        });
    }
}