using Healthcare.Api.Services;

namespace Healthcare.Api.Tests;

public class RagServiceTests
{
    [Fact]
    public void IsRelevant_HighScore_ReturnsTrue()
    {
        Assert.True(
            RagService.IsRelevant(0.73));
    }

    [Fact]
    public void IsRelevant_LowScore_ReturnsFalse()
    {
        Assert.False(
            RagService.IsRelevant(0.39));
    }

    [Fact]
    public void IsRelevant_ThresholdScore_ReturnsTrue()
    {
        Assert.True(
            RagService.IsRelevant(0.55));
    }
}
