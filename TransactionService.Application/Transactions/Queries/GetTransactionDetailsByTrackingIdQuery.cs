using MediatR;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record GetTransactionDetailsByTrackingIdQuery(string TrackingId) : IRequest<TransactionDetailResponse?>;