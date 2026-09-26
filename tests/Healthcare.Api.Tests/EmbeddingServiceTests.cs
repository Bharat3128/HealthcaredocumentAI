using Healthcare.Api.Services;

namespace Healthcare.Api.Tests;

public class EmbeddingServiceTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        var service =
            new EmbeddingService(new HttpClient());

        var first = new float[]
        {
            1f,
            2f,
            3f
        };

        var second = new float[]
        {
            1f,
            2f,
            3f
        };

        var result =
            service.CosineSimilarity(
                first,
                second);

        Assert.Equal(
            1.0,
            result,
            precision: 5);
    }

    [Fact]
    public void CosineSimilarity_DifferentVectorLengths_ReturnsZero()
    {
        var service =
            new EmbeddingService(new HttpClient());

        var first =
            new float[] { 1f, 2f };

        var second =
            new float[] { 1f, 2f, 3f };

        var result =
            service.CosineSimilarity(
                first,
                second);

        Assert.Equal(0, result);
    }
}
