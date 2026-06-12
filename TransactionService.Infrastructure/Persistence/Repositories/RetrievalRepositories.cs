using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;
using TransactionService.Application.Documents.DTOs;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class KnowledgeBaseRepository : IKnowledgeBaseRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<KnowledgeBaseRepository> _logger;
    public KnowledgeBaseRepository(ApplicationDbContext context, ILogger<KnowledgeBaseRepository> logger)
    {
        _context = context;
        _logger = logger;
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

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentRepository> _logger;
    public DocumentRepository(ApplicationDbContext context, ILogger<DocumentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<DocumentResultDto?> GetDocumentByIdAsync(string documentId)
    {
        if (!Guid.TryParse(documentId, out var id))
        {
            _logger.LogWarning("Invalid document ID format: {DocumentId}", documentId);
            return null;
        }

        var documentEntity = await _context.Documents.FirstOrDefaultAsync(d => d.Id == id);

        if (documentEntity == null) return null;

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
                _logger.LogError("File not found at path: {StoragePath}", fullPath);
                fileContent = $"[System Error: File could not be found at path: {documentEntity.StoragePath}]";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file for document ID: {DocumentId}", documentId);
            fileContent = $"[System Error: Failed to read file. {ex.Message}]";
        }

        return new DocumentResultDto(documentEntity.FileName, documentEntity.DocumentType ?? "Unknown", fileContent);
    }

    public async Task<DocumentUploadResponse> CreateDocumentAsync(string fileName, string contentType, Stream fileStream)
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "StoredDocuments");
        if (!Directory.Exists(uploadDir))
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

    public async Task<IEnumerable<DocumentResultDto>> SearchDocumentAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<DocumentResultDto>();

        var lowerQuery = query.ToLower();
        var matchedDocuments = new List<DocumentResultDto>();

        var allDocs = await _context.Documents
        .Select(d => new { d.Id, d.FileName, d.StoragePath, d.DocumentType })
        .ToListAsync();

        foreach (var doc in allDocs)
        {
            double simulatedScore = 0;
            string matchedContentPreview = String.Empty;

            if (doc.FileName != null && doc.FileName.ToLower().Contains(lowerQuery))
            {
                simulatedScore = 0.9;
            }
            else if (!string.IsNullOrWhiteSpace(doc.StoragePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), doc.StoragePath);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        using var streamReader = new StreamReader(fullPath);
                        string? line;

                        while ((line = await streamReader.ReadLineAsync()) != null)
                        {
                            if (line.ToLower().Contains(lowerQuery))
                            {
                                simulatedScore = 0.5;
                                matchedContentPreview = line.Length > 200 ? line.Substring(0, 200) + "..." : line;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error streaming file for document search: {StoragePath}", fullPath);
                    }
                }
            }
            if (simulatedScore > 0.0)
            {
                matchedDocuments.Add(new DocumentResultDto(
                    doc.FileName ?? "No File Name",
                    doc.DocumentType ?? "Unknown",
                    matchedContentPreview
                ));
            }
        }
        return matchedDocuments
        .OrderByDescending(d => 1)
        .Take(5);
    }
}

public class SystemLogRepository : ISystemLogRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SystemLogRepository> _logger;
    public SystemLogRepository(ApplicationDbContext context, ILogger<SystemLogRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<IEnumerable<LogResultDto>> SearchLogsAsync(string query, string dateRange)
    {
        var dbQuery = _context.TransactionLogs.AsQueryable();
        var now = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dateRange))
        {   
            _logger.LogInformation("Applying date range filter: {DateRange} with {Query}", dateRange, query);
            if (dateRange.Contains("last 7 days", StringComparison.OrdinalIgnoreCase))
            {
                var cutoff = now.AddDays(-7);
                dbQuery = dbQuery.Where(log => log.CreatedAt >= cutoff);
            }
            else if (dateRange.Contains("to"))
            {
                var dates = dateRange.Split("to", StringSplitOptions.TrimEntries);
                if (dates.Length == 2 && DateTime.TryParse(dates[0], out var startDate) && DateTime.TryParse(dates[1], out var endDate))
                {
                    dbQuery = dbQuery.Where(log => log.CreatedAt >= startDate && log.CreatedAt <= endDate);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            bool isEnumMatch = Enum.TryParse<EventType>(query, true, out var parsedType);

            dbQuery = dbQuery.Where(l =>
                (l.Message != null && l.Message.Contains(query)) ||
                (l.ProviderResponseCode != null && l.ProviderResponseCode.Contains(query))
                || (isEnumMatch && l.Type == parsedType)
            );
        }

        var logs = await dbQuery.OrderByDescending(log => log.CreatedAt)
            .Take(50).
            ToListAsync();

        return logs.Select(log => new LogResultDto(log.CreatedAt, log.Type.ToString() ?? "INFO", log.Message ?? "No message provided", "System"));
    }
}