# Exception Handler Detailed Implementation

> This document is extracted from SKILL.md `## Core Concepts`, containing complete FluentValidation exception handler implementation and registration order explanation.

## FluentValidation Exception Handler

```csharp
/// <summary>
/// FluentValidation dedicated exception handler
/// </summary>
public class FluentValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<FluentValidationExceptionHandler> _logger;

    public FluentValidationExceptionHandler(ILogger<FluentValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false; // Let next handler process
        }

        _logger.LogWarning(validationException, "Validation failed: {Message}", validationException.Message);

        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Detail = "Input data contains validation errors",
            Instance = httpContext.Request.Path
        };

        foreach (var error in validationException.Errors)
        {
            if (problemDetails.Errors.ContainsKey(error.PropertyName))
            {
                var errors = problemDetails.Errors[error.PropertyName].ToList();
                errors.Add(error.ErrorMessage);
                problemDetails.Errors[error.PropertyName] = errors.ToArray();
            }
            else
            {
                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
        }

        httpContext.Response.StatusCode = 400;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

## Registration Order is Important

Exception handlers execute in registration order, specific handlers must be registered before global handlers:

```csharp
// Program.cs
builder.Services.AddProblemDetails();

// Order matters! Register specific handlers first
builder.Services.AddExceptionHandler<FluentValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Middleware
app.UseExceptionHandler();
```
