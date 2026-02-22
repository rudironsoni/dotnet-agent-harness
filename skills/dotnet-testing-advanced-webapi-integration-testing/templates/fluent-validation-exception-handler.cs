using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourProject.Api.Handlers;

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

    /// <summary>
    /// Try to handle ValidationException
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Only handle FluentValidation's ValidationException
        if (exception is not ValidationException validationException)
        {
            return false; // Let next handler process
        }

        _logger.LogWarning(validationException, "Validation failed: {Message}", validationException.Message);

        var problemDetails = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "The input data contains validation errors, please check and resubmit.",
            Instance = httpContext.Request.Path
        };

        // Convert validation errors to ValidationProblemDetails format
        foreach (var error in validationException.Errors)
        {
            var propertyName = error.PropertyName;
            var errorMessage = error.ErrorMessage;

            if (problemDetails.Errors.ContainsKey(propertyName))
            {
                // If error already exists for this property, add to array
                var existingErrors = problemDetails.Errors[propertyName].ToList();
                existingErrors.Add(errorMessage);
                problemDetails.Errors[propertyName] = existingErrors.ToArray();
            }
            else
            {
                problemDetails.Errors.Add(propertyName, new[] { errorMessage });
            }
        }

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;
    }
}
