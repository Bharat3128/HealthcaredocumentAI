using Healthcare.Api.Services;

namespace Healthcare.Api.Tests;

public class DocumentValidationServiceTests
{
    [Fact]
    public void Validate_ValidClaim_ReturnsValid()
    {
        var service = new DocumentValidationService();

        var json = """
        {
          "claimNumber": "CLM-1001",
          "memberId": "MBR-1001",
          "memberName": "John Test",
          "provider": "Test Medical Center",
          "dateOfService": "09/20/2026",
          "totalCharges": "250.00"
        }
        """;

        var result = service.Validate("Claim", json);

        Assert.Equal("Valid", result);
    }

    [Fact]
    public void Validate_MissingClaimField_ReturnsNeedsReview()
    {
        var service = new DocumentValidationService();

        var json = """
        {
          "claimNumber": "CLM-1001",
          "memberId": "MBR-1001"
        }
        """;

        var result = service.Validate("Claim", json);

        Assert.Equal("NeedsReview", result);
    }

    [Fact]
    public void Validate_NoStructuredData_ReturnsNeedsReview()
    {
        var service = new DocumentValidationService();

        var result = service.Validate("Claim", null);

        Assert.Equal("NeedsReview", result);
    }

    [Fact]
    public void Validate_NonClaim_ReturnsNotValidated()
    {
        var service = new DocumentValidationService();

        var result = service.Validate("Resume", "{}");

        Assert.Equal("NotValidated", result);
    }
}
