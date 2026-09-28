using System.Text.RegularExpressions;
using AI.DocumentClassifier.Application;
using AI.DocumentClassifier.Domain;
using Microsoft.Extensions.Options;

namespace AI.DocumentClassifier.Infrastructure;

public sealed partial class RuleBasedClassificationProvider(IOptions<ClassifierOptions> options) : IClassificationProvider
{
    private const int ExcerptRadius = 45;
    private readonly ClassifierOptions _options = options.Value;

    private static readonly IReadOnlyDictionary<DocumentType, Rule[]> Rules = new Dictionary<DocumentType, Rule[]>
    {
        [DocumentType.Invoice] =
        [
            new("invoice number", 3), new("invoice", 2), new("amount due", 3), new("subtotal", 2),
            new("tax", 1), new("bill to", 2), new("payment due", 2), new("vat", 1.5)
        ],
        [DocumentType.Contract] =
        [
            new("agreement", 2), new("contract", 3), new("hereinafter", 2), new("party", 1),
            new("parties", 1.5), new("terms and conditions", 2), new("termination", 1.5), new("governing law", 2)
        ],
        [DocumentType.CV] =
        [
            new("curriculum vitae", 4), new("resume", 3), new("work experience", 3), new("education", 1.5),
            new("skills", 1), new("employment history", 3), new("professional experience", 3), new("references", 1)
        ],
        [DocumentType.Complaint] =
        [
            new("complaint", 3), new("dissatisfied", 2), new("unacceptable", 2), new("refund", 1.5),
            new("resolve this issue", 2.5), new("customer service", 1), new("formal complaint", 4), new("grievance", 3)
        ],
        [DocumentType.Report] =
        [
            new("report", 2), new("executive summary", 3), new("findings", 2), new("methodology", 2),
            new("analysis", 1), new("recommendations", 2), new("conclusion", 1.5), new("results", 1)
        ]
    };

    public string Name => "local-rules-v1";

    public ValueTask<DocumentClassification> ClassifyAsync(ClassificationInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = WhitespaceRegex().Replace(input.Text, " ").Trim();
        var matches = Rules.ToDictionary(
            item => item.Key,
            item => item.Value
                .Select(rule => Match(normalized, rule))
                .Where(match => match is not null)
                .Cast<RuleMatch>()
                .ToArray());

        var ranked = matches
            .Select(item => new { Type = item.Key, Score = item.Value.Sum(match => match.Rule.Weight), Matches = item.Value })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Type)
            .ToArray();

        var winner = ranked[0];
        if (winner.Score < _options.MinimumScore)
        {
            var otherConfidence = Math.Round(1 - Math.Min(winner.Score / _options.MinimumScore, 0.8), 4);
            return ValueTask.FromResult(new DocumentClassification(DocumentType.Other, otherConfidence, [], Name));
        }

        var runnerUpScore = ranked.Length > 1 ? ranked[1].Score : 0;
        var confidence = Math.Round(winner.Score / (winner.Score + runnerUpScore + 2), 4);
        var evidence = winner.Matches
            .OrderByDescending(match => match.Rule.Weight)
            .ThenBy(match => match.Index)
            .Take(_options.MaxEvidenceItems)
            .Select(match => new ClassificationEvidence(
                match.Rule.Term,
                match.Rule.Weight,
                CreateExcerpt(normalized, match.Index, match.Rule.Term.Length)))
            .ToArray();

        return ValueTask.FromResult(new DocumentClassification(winner.Type, confidence, evidence, Name));
    }

    private static RuleMatch? Match(string text, Rule rule)
    {
        var match = Regex.Match(
            text,
            $@"(?<![\p{{L}}\p{{N}}]){Regex.Escape(rule.Term)}(?![\p{{L}}\p{{N}}])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? new RuleMatch(rule, match.Index) : null;
    }

    private static string CreateExcerpt(string text, int index, int length)
    {
        var start = Math.Max(0, index - ExcerptRadius);
        var end = Math.Min(text.Length, index + length + ExcerptRadius);
        var prefix = start > 0 ? "…" : string.Empty;
        var suffix = end < text.Length ? "…" : string.Empty;
        return $"{prefix}{text[start..end].Trim()}{suffix}";
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    private sealed record Rule(string Term, double Weight);
    private sealed record RuleMatch(Rule Rule, int Index);
}
