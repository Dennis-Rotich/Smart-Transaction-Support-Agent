using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionStatusQuery(string Reference) : IRequest<TransactionResponseGet?>;