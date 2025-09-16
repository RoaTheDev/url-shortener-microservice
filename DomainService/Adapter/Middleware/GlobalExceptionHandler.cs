using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace DomainService.Adapter.Middleware;

internal sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private static readonly ILogger Logger = Log.ForContext<GlobalExceptionHandler>();

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, clientMessage, logAsError) = exception switch
        {
            TimeoutException => (StatusCodes.Status504GatewayTimeout, "Service temporarily unavailable.", false),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Resource conflict.", false),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required.", false),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", false),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed.", false),
            _ => (StatusCodes.Status500InternalServerError, "An un-handled error occurred.", true)
        };

        var correlationId = httpContext.TraceIdentifier;
        var logProperties = new
        {
            QueryString = httpContext.Request.QueryString.Value,
            StatusCode = statusCode,
            ExceptionSource = exception.Source,
            UserId = httpContext.User.Identity?.Name ?? "Anonymous",
            UserIP = httpContext.Connection.RemoteIpAddress?.ToString(),
            TraceId = httpContext.Features.Get<IHttpActivityFeature>()?.Activity.Id,
            Details = exception.Message,
            CorrelationId = correlationId,
        };

        if (logAsError)
        {
            Logger.Error(exception,
                "Unhandled exception occurred: {ExceptionType} in {HttpMethod} {RequestPath} : {@Logs}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                logProperties);
        }
        else
        {
            Logger.Warning(exception,
                "Expected exception handled: {ExceptionType} in {HttpMethod} {RequestPath} {@Logs} ",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                logProperties);
        }

        ProblemDetails problemDetails;

        if (exception is ValidationException validationException)
        {
            var fieldErrors = new Dictionary<string, string[]>
            {
                { validationException.TargetSite?.Name ?? "Field", [validationException.Message] }
            };

            problemDetails = new ValidationProblemDetails(fieldErrors)
            {
                Type = GetErrorType(statusCode),
                Title = "Validation error",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            };
        }
        else
        {
            problemDetails = new ProblemDetails
            {
                Type = GetErrorType(statusCode),
                Title = logAsError ? "An unexpected error occurred" : "An error occurred",
                Detail = clientMessage,
                Status = statusCode
            };
        }

        problemDetails.Extensions["correlationId"] = correlationId;
        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
    }

    private static string GetErrorType(int statusCode) => statusCode switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        409 => "Conflict",
        422 => "Unprocessable Entity",
        500 => "Internal Server Error",
        504 => "Gateway Timeout",
        _ => "Unknown Error"
    };
}