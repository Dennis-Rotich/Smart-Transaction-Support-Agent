using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionDetailsByTransactionReferenceQuery(string Reference) : IRequest<TransactionDetailResponse?>;