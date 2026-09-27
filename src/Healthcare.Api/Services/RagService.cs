using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services;

public class RagSource
{
    public int ChunkIndex { get; set; }
    public double Score { get; set; }
    public string Excerpt { get; set; } = string.Empty;
}

public class RagResponse
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public List<RagSource> Sources { get; set; } = new();
}

public class RagService
{
    private readonly DocumentRetrievalService _retrievalService;
    private readonly IChatService _chatService;
    private readonly RagOptions _options;

    public RagService(
        DocumentRetrievalService retrievalService,
        IChatService chatService,
        IOptions<RagOptions> options)
    {
        _retrievalService = retrievalService;
        _chatService = chatService;
        _options = options.Value;
    }

    public async Task<RagResponse> AskAsync(
        Guid documentId,
        string question,
        CancellationToken cancellationToken = default)
    {
        var results =
            await _retrievalService.SemanticSearchAsync(
                documentId,
                question);

        var topResults = results
            .Take(_options.TopK)
            .ToList();

        if (topResults.Count == 0 ||
            !IsRelevant(
                topResults[0].Score,
                _options.MinimumScore))
        {
            return new RagResponse
            {
                Question = question,
                Answer =
                    "The answer was not found in the document.",
                Sources = new List<RagSource>()
            };
        }

        var context = string.Join(
            Environment.NewLine +
            Environment.NewLine,
            topResults.Select(result =>
                result.Content));

        var answer =
            await _chatService.GenerateAnswerAsync(
                question,
                context,
                cancellationToken);

        var sources = topResults
            .Select(result => new RagSource
            {
                ChunkIndex = result.ChunkIndex,
                Score = Math.Round(result.Score, 3),
                Excerpt = CreateExcerpt(
                    result.Content,
                    250)
            })
            .ToList();

        return new RagResponse
        {
            Question = question,
            Answer = answer,
            Sources = sources
        };
    }

    public static bool IsRelevant(
        double score,
        double minimumScore = 0.55)
    {
        return score >= minimumScore;
    }

    private static string CreateExcerpt(
        string content,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var clean = content
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        if (clean.Length <= maxLength)
        {
            return clean;
        }

        return clean[..maxLength] + "...";
    }
}


