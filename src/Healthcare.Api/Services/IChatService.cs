namespace Healthcare.Api.Services;

public interface IChatService
{
    Task<string> GenerateAnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default);
}
