using MediatR;

namespace TransactionService.Application.Transactions.Commands;

public record ProcessIpnCommand(string OrderTrackingId, string IpnNotificationId) : IRequest<bool>;

