namespace Healthcare.Api.Options;

public class AzureOpenAIOptions
{
    public bool Enabled { get; set; } = false;
    public string Endpoint { get; set; } = string.Empty;
    public string ChatDeployment { get; set; } = string.Empty;
    public string EmbeddingDeployment { get; set; } = string.Empty;
}
