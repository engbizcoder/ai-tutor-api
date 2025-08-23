namespace Ai.Tutor.Api.Services;

using System.ComponentModel.DataAnnotations;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class ProblemDetailsSetupExtensions
{
    private static readonly Action<ILogger, int, string?, string?, string?, Exception?> Problem4XxLogged =
        LoggerMessage.Define<int, string?, string?, string?>(
            LogLevel.Warning,
            new EventId(3001, nameof(Problem4XxLogged)),
            "ProblemDetails 4xx. Status={Status}, Title={Title}, Path={Path}, CorrelationId={CorrelationId}");

    private static readonly Action<ILogger, int, string?, string?, string?, Exception?> Problem5XxLogged =
        LoggerMessage.Define<int, string?, string?, string?>(
            LogLevel.Error,
            new EventId(3002, nameof(Problem5XxLogged)),
            "ProblemDetails 5xx. Status={Status}, Title={Title}, Path={Path}, CorrelationId={CorrelationId}");

    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services, IHostEnvironment env)
    {
        services.AddProblemDetails(
            options =>
        {
            options.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment();

            // Map common exceptions to HTTP status codes
            options.Map<KeyNotFoundException>(ex => new ProblemDetailsException(StatusCodes.Status404NotFound, ex.Message));
            options.Map<UnauthorizedAccessException>(ex => new ProblemDetailsException(StatusCodes.Status401Unauthorized, ex.Message));
            options.Map<ValidationException>(ex => new ProblemDetailsException(StatusCodes.Status400BadRequest, ex.Message));
            options.Map<ArgumentException>(ex => new ProblemDetailsException(StatusCodes.Status400BadRequest, ex.Message));

            // Fallback
            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);

            // Centralized logging when ProblemDetails is about to be written
            options.OnBeforeWriteDetails = (ctx, details) =>
            {
                var factory = ctx.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = factory.CreateLogger("ProblemDetails");

                var status = details.Status ?? StatusCodes.Status500InternalServerError;
                var title = details.Title;
                var path = ctx.Request.Path.HasValue ? ctx.Request.Path.Value : string.Empty;
                var correlationId = ctx.Response.Headers.TryGetValue(Middlewares.CorrelationIdMiddleware.HeaderName, out var id)
                    ? id.ToString()
                    : ctx.TraceIdentifier;

                // Set instance to the request path
                details.Instance = path;

                // Provide stable code and retryable flags for clients
                var code = status switch
                {
                    StatusCodes.Status400BadRequest => "BAD_REQUEST",
                    StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
                    StatusCodes.Status403Forbidden => "FORBIDDEN",
                    StatusCodes.Status404NotFound => "NOT_FOUND",
                    StatusCodes.Status409Conflict => "CONFLICT",
                    StatusCodes.Status422UnprocessableEntity => "SEMANTIC_ERROR",
                    StatusCodes.Status429TooManyRequests => "RATE_LIMIT",
                    _ when status >= 500 => "INTERNAL_ERROR",
                    _ => "ERROR",
                };

                var retryable = status is StatusCodes.Status429TooManyRequests or >= 500;

                details.Extensions["code"] = code;
                details.Extensions["retryable"] = retryable;
                details.Extensions["correlationId"] = correlationId;

                if (status >= 500)
                {
                    Problem5XxLogged(logger, status, title, path, correlationId, null);
                }
                else
                {
                    Problem4XxLogged(logger, status, title, path, correlationId, null);
                }
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiProblemDetails(this IApplicationBuilder app)
        => app.UseProblemDetails();
}
