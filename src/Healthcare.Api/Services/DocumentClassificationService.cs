namespace Healthcare.Api.Services;

public class DocumentClassificationService
{
    public string Classify(string extractedText)
    {
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return "Other";
        }

        var text = extractedText.ToLowerInvariant();

        if (text.Contains("explanation of benefits") ||
            text.Contains("eob"))
        {
            return "EOB";
        }

        if (text.Contains("prior authorization") ||
            text.Contains("preauthorization"))
        {
            return "Prior Authorization";
        }

        if (text.Contains("claim number") ||
            text.Contains("claim id") ||
            text.Contains("claims"))
        {
            return "Claim";
        }

        if (text.Contains("medical record") ||
            text.Contains("patient") ||
            text.Contains("diagnosis"))
        {
            return "Medical Record";
        }

        if (text.Contains("professional experience") ||
            text.Contains("education") ||
            text.Contains("skills"))
        {
            return "Resume";
        }

        return "Other";
    }
}
