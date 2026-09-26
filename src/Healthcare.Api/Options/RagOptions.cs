namespace Healthcare.Api.Options;

public class RagOptions
{
    public double MinimumScore { get; set; } = 0.55;

    public int TopK { get; set; } = 3;
}
