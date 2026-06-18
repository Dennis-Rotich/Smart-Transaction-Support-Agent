namespace TransactionService.Application.Interfaces;

public interface IPdfExtractionService 
{
	Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken=default);
}