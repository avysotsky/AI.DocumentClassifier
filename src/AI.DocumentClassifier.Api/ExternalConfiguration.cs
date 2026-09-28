namespace AI.DocumentClassifier.Api;

public static class ExternalConfiguration
{
    public static IConfigurationBuilder AddExternalConfig(this IConfigurationBuilder builder, string contentRootPath)
    {
        var configuredPath = Environment.GetEnvironmentVariable("DOCUMENT_CLASSIFIER_CONFIG_PATH");
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(contentRootPath, "config.json")
            : configuredPath;
        return builder.AddJsonFile(path, optional: true, reloadOnChange: true);
    }
}
