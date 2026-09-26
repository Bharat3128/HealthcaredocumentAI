namespace Healthcare.Api.Options;

public class ChunkingOptions
{
    public int ChunkSize { get; set; } = 800;

    public int Overlap { get; set; } = 100;
}
