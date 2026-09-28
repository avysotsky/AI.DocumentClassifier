using AI.DocumentClassifier.Application;
using AI.DocumentClassifier.Domain;
using AI.DocumentClassifier.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace AI.DocumentClassifier.UnitTests;

public sealed class RuleBasedClassificationProviderTests
{
    private readonly RuleBasedClassificationProvider _provider =
        new(Options.Create(new ClassifierOptions()));

    [Theory]
    [InlineData("INVOICE Invoice number 42. Bill to Acme. Amount due: 120 EUR.", DocumentType.Invoice)]
    [InlineData("SERVICE AGREEMENT between the parties. Governing law and termination terms follow.", DocumentType.Contract)]
    [InlineData("CURRICULUM VITAE. Professional experience, education and skills.", DocumentType.CV)]
    [InlineData("Formal complaint: I am dissatisfied and request that you resolve this issue.", DocumentType.Complaint)]
    [InlineData("Quarterly report. Executive summary, methodology, findings and recommendations.", DocumentType.Report)]
    public async Task ClassifyAsyncStrongSignalsReturnsExpectedType(string text, DocumentType expected)
    {
        var result = await _provider.ClassifyAsync(new ClassificationInput(text, null));

        Assert.Equal(expected, result.Type);
        Assert.InRange(result.Confidence, 0.01, 1);
        Assert.NotEmpty(result.Evidence);
        Assert.Equal("local-rules-v1", result.Provider);
    }

    [Fact]
    public async Task ClassifyAsyncWithoutKnownSignalsReturnsOther()
    {
        var result = await _provider.ClassifyAsync(
            new ClassificationInput("A short note about tomorrow's team lunch.", "note.txt"));

        Assert.Equal(DocumentType.Other, result.Type);
        Assert.Empty(result.Evidence);
    }

    [Fact]
    public async Task ClassifyAsyncIsCaseAndWhitespaceInsensitiveAndDeterministic()
    {
        var input = new ClassificationInput("  AMOUNT\r\n DUE for this InVoice Number 88 and SUBTOTAL  ", null);

        var first = await _provider.ClassifyAsync(input);
        var second = await _provider.ClassifyAsync(input);

        Assert.Equal(first.Type, second.Type);
        Assert.Equal(first.Confidence, second.Confidence);
        Assert.Equal(first.Provider, second.Provider);
        Assert.Equal(first.Evidence, second.Evidence);
        Assert.Equal(DocumentType.Invoice, first.Type);
    }

    [Fact]
    public async Task ClassifyAsyncUsesWholeTermsOnly()
    {
        var result = await _provider.ClassifyAsync(
            new ClassificationInput("The partygoer studied taxation methodologycal notes.", null));

        Assert.Equal(DocumentType.Other, result.Type);
    }

    [Fact]
    public async Task ClassifyAsyncThrowsWhenCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await _provider.ClassifyAsync(
                new ClassificationInput("Invoice amount due", null),
                source.Token));
    }
}
