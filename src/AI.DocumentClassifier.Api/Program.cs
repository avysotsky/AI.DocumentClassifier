using System.Text.Json.Serialization;
using AI.DocumentClassifier.Api;
using AI.DocumentClassifier.Application;
using AI.DocumentClassifier.Infrastructure;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddExternalConfig(builder.Environment.ContentRootPath);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDocumentClassification(builder.Configuration);
builder.Services.AddHealthChecks().AddCheck<ClassifierHealthCheck>("classifier", tags: ["ready"]);

var app = builder.Build();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = registration => registration.Tags.Contains("ready") });
app.MapPost("/classifications", async (
    [FromBody] ClassifyDocumentRequest request,
    IDocumentClassifier classifier,
    CancellationToken token) =>
{
    var result = await classifier.ClassifyAsync(new(request.Text, request.FileName), token);
    return Results.Ok(new ClassifyDocumentResponse(
        request.DocumentId,
        request.FileName,
        result.Type,
        result.Confidence,
        result.Evidence
            .Select(item => new ClassificationEvidenceResponse(item.Term, item.Weight, item.Excerpt))
            .ToArray(),
        result.Provider));
})
    .AddEndpointFilter<RequestValidationFilter<ClassifyDocumentRequest>>()
    .WithName("ClassifyDocument")
    .WithSummary("Classifies document text into one supported document type.")
    .Produces<ClassifyDocumentResponse>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();

public partial class Program;
