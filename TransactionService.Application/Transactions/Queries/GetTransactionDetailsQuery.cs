using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionDetailsQuery(string Reference) : IRequest<TransactionDetailResponse?>;