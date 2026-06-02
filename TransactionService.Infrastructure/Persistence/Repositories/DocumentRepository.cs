using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;
using TransactionService.Application.Documents.DTOs;
using TransactionService.Domain.Entities;
using System.Runtime.InteropServices;

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

    public async Task<DocumentUploadResponse> CreateDocumentAsync(string fileName, string contentType, Stream fileStream)
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "StoredDocuments");
        if(!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        var fileExt = Path.GetExtension(fileName);
        var safeFileName = $"{Guid.NewGuid()}{fileExt}";
        var physicalPath = Path.Combine(uploadDir, safeFileName);

        using (var diskStream = new FileStream(physicalPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(diskStream);
        }

        var document = new Document(fileName, physicalPath, contentType);
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return new DocumentUploadResponse(document.FileName, document.StoragePath, document.DocumentType ?? "Unknown");
    }
}