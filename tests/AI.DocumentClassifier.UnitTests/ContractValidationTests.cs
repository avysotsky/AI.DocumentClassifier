using System.ComponentModel.DataAnnotations;
using AI.DocumentClassifier.Api;
using Xunit;

namespace AI.DocumentClassifier.UnitTests;

public sealed class ContractValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData("         ")]
    [InlineData("too short")]
    public void RequestInvalidTextFailsValidation(string text)
    {
        var request = new ClassifyDocumentRequest { Text = text };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            results,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void RequestWhitespaceFileNameFailsValidation()
    {
        var request = new ClassifyDocumentRequest { Text = "Invoice number 123", FileName = " " };

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            [],
            validateAllProperties: true);

        Assert.False(isValid);
    }
}
