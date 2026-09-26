namespace Healthcare.Api.Options;

public class OllamaOptions
{
    public string BaseUrl { get; set; } =
        "http://localhost:11434";

    public string ChatModel { get; set; } =
        "llama3.2:3b";

    public string EmbeddingModel { get; set; } =
        "nomic-embed-text";

    public int TimeoutSeconds { get; set; } = 120;
}
