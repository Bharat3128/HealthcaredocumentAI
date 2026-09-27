namespace Healthcare.Api.Options;

public class DocumentIntelligenceOptions
{
    public bool Enabled { get; set; } = false;
    public string Endpoint { get; set; } = string.Empty;
    public string ModelId { get; set; } = "prebuilt-read";
}
