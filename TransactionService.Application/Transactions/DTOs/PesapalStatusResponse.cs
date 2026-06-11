namespace TransactionService.Application.Transactions.DTOs;

public record PesapalStatusResponse
(
	string PaymentStatusDescription,
	string? PaymentMethod,
	string? ConfirmationCode,
	string? PaymentAccount,
	string? Description);
