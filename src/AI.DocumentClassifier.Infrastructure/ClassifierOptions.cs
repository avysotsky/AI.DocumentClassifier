using System.ComponentModel.DataAnnotations;

namespace AI.DocumentClassifier.Infrastructure;

public sealed class ClassifierOptions
{
    public const string SectionName = "Classifier";

    [Range(0.1, 100)]
    public double MinimumScore { get; init; } = 2;

    [Range(1, 20)]
    public int MaxEvidenceItems { get; init; } = 5;
}
