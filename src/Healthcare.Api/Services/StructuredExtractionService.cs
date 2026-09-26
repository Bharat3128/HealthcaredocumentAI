using System.Text.Json;
using System.Text.RegularExpressions;

namespace Healthcare.Api.Services;

public class StructuredExtractionService
{
    public string Extract(string? extractedText, string? documentType)
    {
        var result = new Dictionary<string, object?>();

        result["documentType"] = documentType ?? "Other";

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return JsonSerializer.Serialize(result);
        }

        if (documentType == "Resume")
        {
            result["email"] = ExtractValue(
                extractedText,
                @"([\w\.-]+@[\w\.-]+\.\w+)");

            result["containsSkills"] =
                extractedText.Contains(
                    "skills",
                    StringComparison.OrdinalIgnoreCase);
        }

        if (documentType == "Claim")
        {
            result["claimNumber"] = ExtractValue(
                extractedText,
                @"Claim\s*(?:Number|ID)\s*[:#-]?\s*([A-Z0-9-]+)");

            result["memberId"] = ExtractValue(
                extractedText,
                @"Member\s*ID\s*[:#-]?\s*([A-Z0-9-]+)");

            result["memberName"] = ExtractValue(
                extractedText,
                @"Member\s*Name\s*:\s*(.*?)\s+Provider\s*:");

            result["provider"] = ExtractValue(
                extractedText,
                @"Provider\s*:\s*(.*?)\s+Date\s*of\s*Service\s*:");

            result["dateOfService"] = ExtractValue(
                extractedText,
                @"Date\s*of\s*Service\s*:\s*(\d{1,2}/\d{1,2}/\d{4})");

            result["diagnosis"] = ExtractValue(
                extractedText,
                @"Diagnosis\s*:\s*(.*?)\s+Procedure\s*Code\s*:");

            result["procedureCode"] = ExtractValue(
                extractedText,
                @"Procedure\s*Code\s*:\s*([A-Z0-9-]+)");

            result["totalCharges"] = ExtractValue(
                extractedText,
                @"Total\s*Charges\s*:\s*\$?([\d,]+\.\d{2})");

            result["insurancePaid"] = ExtractValue(
                extractedText,
                @"Insurance\s*Paid\s*:\s*\$?([\d,]+\.\d{2})");

            result["patientResponsibility"] = ExtractValue(
                extractedText,
                @"Patient\s*Responsibility\s*:\s*\$?([\d,]+\.\d{2})");
        }

        return JsonSerializer.Serialize(result);
    }

    private string? ExtractValue(string text, string pattern)
    {
        var match = Regex.Match(
            text,
            pattern,
            RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            return null;
        }

        if (match.Groups.Count > 1)
        {
            return match.Groups[1].Value.Trim();
        }

        return match.Value.Trim();
    }
}
