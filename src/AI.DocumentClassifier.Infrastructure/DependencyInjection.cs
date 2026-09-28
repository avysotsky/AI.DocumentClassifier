using AI.DocumentClassifier.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AI.DocumentClassifier.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentClassification(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ClassifierOptions>()
            .Bind(configuration.GetSection(ClassifierOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IClassificationProvider, RuleBasedClassificationProvider>();
        services.AddSingleton<IDocumentClassifier, DocumentClassifierService>();
        return services;
    }
}
