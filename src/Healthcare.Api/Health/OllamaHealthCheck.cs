using Healthcare.Api.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Healthcare.Api.Health;

public class OllamaHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OllamaOptions _options;

    public OllamaHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptions<OllamaOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeoutTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutTokenSource.CancelAfter(
                TimeSpan.FromSeconds(5));

            var client =
                _httpClientFactory.CreateClient();

            var url =
                $"{_options.BaseUrl.TrimEnd('/')}/api/tags";

            using var response =
                await client.GetAsync(
                    url,
                    timeoutTokenSource.Token);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy(
                    "Ollama is available.");
            }

            return HealthCheckResult.Unhealthy(
                $"Ollama returned HTTP {(int)response.StatusCode}.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy(
                "Ollama health check timed out.");
        }
        catch (HttpRequestException)
        {
            return HealthCheckResult.Unhealthy(
                "Ollama is unavailable.");
        }
    }
}
