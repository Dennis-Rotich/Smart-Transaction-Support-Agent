using MediatR;
using TransactionService.Application.Contracts;

namespace TransactionService.Application.Transactions.Commands;

public record GenerateLinkCommand(string Email, string Currency, decimal Amount) : IRequest<PaymentLinkResponse>;