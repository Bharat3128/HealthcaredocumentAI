namespace Healthcare.Api.Options;

public class AzureStorageOptions
{
    public bool Enabled { get; set; } = false;
    public string ServiceUri { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "healthcare-documents";
}
