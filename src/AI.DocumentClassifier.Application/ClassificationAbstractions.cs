using AI.DocumentClassifier.Domain;

namespace AI.DocumentClassifier.Application;

public sealed record ClassificationInput(string Text, string? FileName);

public interface IDocumentClassifier
{
    ValueTask<DocumentClassification> ClassifyAsync(ClassificationInput input, CancellationToken cancellationToken = default);
}

// Implement this interface for a future explicitly configured ONNX or other model provider.
public interface IClassificationProvider
{
    string Name { get; }
    ValueTask<DocumentClassification> ClassifyAsync(ClassificationInput input, CancellationToken cancellationToken = default);
}

public sealed class DocumentClassifierService(IClassificationProvider provider) : IDocumentClassifier
{
    public ValueTask<DocumentClassification> ClassifyAsync(ClassificationInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();
        return provider.ClassifyAsync(input, cancellationToken);
    }
}
