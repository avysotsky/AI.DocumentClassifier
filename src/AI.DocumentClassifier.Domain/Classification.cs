namespace AI.DocumentClassifier.Domain;

public enum DocumentType
{
    Invoice,
    Contract,
    CV,
    Complaint,
    Report,
    Other
}

public sealed record ClassificationEvidence(string Term, double Weight, string Excerpt);

public sealed record DocumentClassification(
    DocumentType Type,
    double Confidence,
    IReadOnlyList<ClassificationEvidence> Evidence,
    string Provider);
