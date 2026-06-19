using System.Text;
using UglyToad.PdfPig;
using TransactionService.Application.Interfaces;

namespace TransactionService.Infrastructure.Integrations;

public class PdfExtractionService : IPdfExtractionService
{
    public Task<string> ExtractTextAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        return Task.Run<string>(() => 
        {
            var textBuilder = new StringBuilder();

            using var document = PdfDocument.Open(pdfStream);

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bottomMargin = page.Height * 0.08;
                var topMargin = page.Height * 0.92;

                var validWords = page.GetWords()
                    .Where(w => w.BoundingBox.Bottom > bottomMargin && w.BoundingBox.Top > topMargin)
                    .OrderByDescending(w => w.BoundingBox.Top)
                    .ThenBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text);

                var cleanPageText = string.Join(" ", validWords);

                if (!string.IsNullOrWhiteSpace(cleanPageText))
                {
                    textBuilder.AppendLine(cleanPageText);
                }
            }
            return textBuilder.ToString();
        }, cancellationToken);
    }
}