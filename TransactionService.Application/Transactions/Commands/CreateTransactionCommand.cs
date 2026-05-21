using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Commands;

public record CreateTransactionCommand(decimal Amount, string Currency) : IRequest<TransactionResponseCreate>;