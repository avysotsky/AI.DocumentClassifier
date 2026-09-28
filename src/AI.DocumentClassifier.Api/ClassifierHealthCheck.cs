using AI.DocumentClassifier.Application;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AI.DocumentClassifier.Api;

public sealed class ClassifierHealthCheck(IClassificationProvider provider) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = string.IsNullOrWhiteSpace(provider.Name)
            ? HealthCheckResult.Unhealthy("No classification provider is configured.")
            : HealthCheckResult.Healthy($"Classification provider '{provider.Name}' is ready.");
        return Task.FromResult(result);
    }
}
