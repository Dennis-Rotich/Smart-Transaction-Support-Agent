using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionDetailsByMerchantReferenceQuery(string Reference) : IRequest<TransactionDetailResponse?>;