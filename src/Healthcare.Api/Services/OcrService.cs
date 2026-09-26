using System.Text;
using UglyToad.PdfPig;

namespace Healthcare.Api.Services;

public class OcrService
{
    public Task<string> ExtractTextAsync(Stream stream)
    {
        if (stream == null || !stream.CanRead)
        {
            throw new InvalidDataException(
                "The PDF stream cannot be read.");
        }

        var extractedText = new StringBuilder();

        using var pdfDocument = PdfDocument.Open(stream);

        foreach (var page in pdfDocument.GetPages())
        {
            var words = page.GetWords()
                .Select(word => word.Text);

            extractedText.AppendLine(
                string.Join(" ", words));
        }

        var text = extractedText.ToString().Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(
                "No readable text was found in this PDF.");
        }

        return Task.FromResult(text);
    }

    public async Task<string> ExtractTextAsync(
        string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "The PDF file could not be found.",
                filePath);
        }

        await using var stream =
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);

        return await ExtractTextAsync(stream);
    }
}
