using System.Net.Http.Json;
using System.Text.Json;
using Healthcare.Api.Exceptions;
using Healthcare.Api.Options;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateAnswerAsync(
        string question,
        string context,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = _options.ChatModel,
            stream = false,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content =
                        "Answer only from the provided context. " +
                        "If the answer is not in the context, say you do not have enough information."
                },
                new
                {
                    role = "user",
                    content =
                        $"Context:\n{context}\n\nQuestion:\n{question}"
                }
            }
        };

        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "/api/chat",
                    request,
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            using var json =
                await response.Content.ReadFromJsonAsync<JsonDocument>(
                    cancellationToken: cancellationToken);

            return json?
                .RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? "No response was generated.";
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            throw new AiServiceUnavailableException(
                "AI service request timed out.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new AiServiceUnavailableException(
                "AI service is currently unavailable.",
                exception);
        }
    }
}
