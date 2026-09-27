using Azure.AI.OpenAI;
using Azure.Identity;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace Healthcare.Api.Services;

public class AzureOpenAIChatService : IChatService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAIChatService(
        IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
        {
            throw new InvalidOperationException(
                "Azure OpenAI endpoint is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ChatDeployment))
        {
            throw new InvalidOperationException(
                "Azure OpenAI chat deployment is not configured.");
        }

        var azureClient = new AzureOpenAIClient(
            new Uri(settings.Endpoint),
            new DefaultAzureCredential());

        _chatClient =
            azureClient.GetChatClient(
                settings.ChatDeployment);
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "Answer only from the supplied document context. " +
                "If the answer is not present in the context, say that " +
                "the information was not found in the document."),

            new UserChatMessage(
                $"Context:\n{context}\n\nQuestion:\n{question}")
        };

        var response =
            await _chatClient.CompleteChatAsync(
                messages,
                cancellationToken: cancellationToken);

        var answer =
            response.Value.Content
                .FirstOrDefault()?.Text?
                .Trim();

        return string.IsNullOrWhiteSpace(answer)
            ? "No answer was generated."
            : answer;
    }
}
