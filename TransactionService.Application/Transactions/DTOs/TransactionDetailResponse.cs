namespace TransactionService.Application.Transactions.DTOs;

public record TransactionDetailResponse(
	string Reference,
	decimal Amount,
	string Currency,
	string Status,
	DateTime CreatedAt,	
	IEnumerable<TransactionLogDto> Logs,
	string? ExternalTrackingId
	);