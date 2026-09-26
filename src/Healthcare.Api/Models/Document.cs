namespace Healthcare.Api.Models;

public class Document
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public string? ExtractedText { get; set; }

    public string? DocumentType { get; set; }

    public string? StructuredDataJson { get; set; }

    public string? ValidationStatus { get; set; }

    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? ProcessingCompletedAt { get; set; }
    public string? ProcessingError { get; set; }
}

