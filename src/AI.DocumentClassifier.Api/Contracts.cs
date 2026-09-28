using System.ComponentModel.DataAnnotations;
using AI.DocumentClassifier.Domain;

namespace AI.DocumentClassifier.Api;

public sealed class ClassifyDocumentRequest : IValidatableObject
{
    public Guid? DocumentId { get; init; }

    [StringLength(260)]
    public string? FileName { get; init; }

    [Required, StringLength(100_000, MinimumLength = 10)]
    public required string Text { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            yield return new ValidationResult("Text must contain non-whitespace characters.", [nameof(Text)]);
        }

        if (FileName is not null && string.IsNullOrWhiteSpace(FileName))
        {
            yield return new ValidationResult("FileName must be null or contain non-whitespace characters.", [nameof(FileName)]);
        }
    }
}

public sealed record ClassificationEvidenceResponse(string Term, double Weight, string Excerpt);

public sealed record ClassifyDocumentResponse(
    Guid? DocumentId,
    string? FileName,
    DocumentType Type,
    double Confidence,
    IReadOnlyList<ClassificationEvidenceResponse> Evidence,
    string Provider);
