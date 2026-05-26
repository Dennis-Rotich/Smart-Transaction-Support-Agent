using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;
    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<DocumentResultDto?> GetDocumentByIdAsync(string documentId)
    {
        if(!Guid.TryParse(documentId, out var id))
        {
            return null;
        }

        var documentEntity = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);

        if(documentEntity == null) return null;

        string fileContent;
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), documentEntity.StoragePath);

            if (File.Exists(fullPath))
            {
                fileContent = await File.ReadAllTextAsync(fullPath);
            }
            else
            {
                fileContent = $"[System Error: File could not be found at path: {documentEntity.StoragePath}]";
            }
        }
        catch (Exception ex)
        {
            fileContent = $"[System Error: Failed to read file. {ex.Message}]";
        }

        return new DocumentResultDto(documentEntity.FileName, documentEntity.DocumentType ?? "Unknown", fileContent);
    }
}