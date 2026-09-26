using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Healthcare.Api.Tests;

public class DocumentsApiIntegrationTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public DocumentsApiIntegrationTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Ask_EmptyQuestion_ReturnsBadRequest()
    {
        var documentId = Guid.NewGuid();

        var response = await _client.PostAsJsonAsync(
            $"/api/Documents/{documentId}/ask",
            new
            {
                question = ""
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}
