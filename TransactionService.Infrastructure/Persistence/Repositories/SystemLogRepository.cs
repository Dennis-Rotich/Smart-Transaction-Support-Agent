using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TransactionService.Application.Interfaces;
using TransactionService.Application.Transactions.Queries;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Infrastructure.Persistence.Repositories;

public class SystemLogRepository : ISystemLogRepository
{
    private readonly ApplicationDbContext _context;
    public SystemLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<LogResultDto>> SearchLogsAsync(string query, string dateRange)
    {
        var dbQuery = _context.TransactionLogs.AsQueryable();

        var now = DateTime.UtcNow;
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

        if(!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(l =>
                (l.Message != null && l.Message.Contains(query)) ||
                (l.ProviderResponseCode != null && l.ProviderResponseCode.Contains(query)) ||
                (l.Type.ToString().Contains(query)));
        }

        var logs = await dbQuery.OrderByDescending(log => log.CreatedAt)
            .Take(50).
            ToListAsync();

        return logs.Select(log => new LogResultDto(log.CreatedAt, log.Type.ToString() ?? "INFO", log.Message ?? "No message provided", "System"));
    }
}