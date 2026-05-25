using MediatR;
using System.Collections.Generic;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record ListRecentTransactionsQuery(int Limit = 10) : IRequest<IEnumerable<RecentTransactionResponse>>;