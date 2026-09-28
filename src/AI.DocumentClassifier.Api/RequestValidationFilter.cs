using System.ComponentModel.DataAnnotations;

namespace AI.DocumentClassifier.Api;

public sealed class RequestValidationFilter<T> : IEndpointFilter where T : class
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
        {
            return next(context);
        }

        var results = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true))
        {
            return next(context);
        }

        var errors = results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty)
                .Select(member => (Member: member, Message: result.ErrorMessage ?? "Invalid value.")))
            .GroupBy(item => item.Member)
            .ToDictionary(group => group.Key, group => group.Select(item => item.Message).Distinct().ToArray());

        return ValueTask.FromResult<object?>(Results.ValidationProblem(errors));
    }
}
