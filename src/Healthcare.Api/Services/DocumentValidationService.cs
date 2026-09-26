using System.Text.Json;

namespace Healthcare.Api.Services;

public class DocumentValidationService
{
    public string Validate(
        string? documentType,
        string? structuredDataJson)
    {
        if (string.IsNullOrWhiteSpace(structuredDataJson))
        {
            return "NeedsReview";
        }

        if (documentType != "Claim")
        {
            return "NotValidated";
        }

        try
        {
            using var jsonDocument =
                JsonDocument.Parse(structuredDataJson);

            var root = jsonDocument.RootElement;

            var requiredFields = new[]
            {
                "claimNumber",
                "memberId",
                "memberName",
                "provider",
                "dateOfService",
                "totalCharges"
            };

            foreach (var field in requiredFields)
            {
                if (!root.TryGetProperty(field, out var value) ||
                    string.IsNullOrWhiteSpace(value.GetString()))
                {
                    return "NeedsReview";
                }
            }

            return "Valid";
        }
        catch (JsonException)
        {
            return "NeedsReview";
        }
    }
}
