namespace Ai.Tutor.Api.Services;

using System.ComponentModel.DataAnnotations;
using Domain.Exceptions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class ProblemDetailsSetupExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services, IHostEnvironment env)
    {
        services.AddProblemDetails(
            options =>
        {
            options.IncludeExceptionDetails = (ctx, ex) => env.IsDevelopment();

            // Map structured API exceptions using their metadata
            options.Map<BaseApiException>(ex => new ProblemDetailsException(ex.StatusCode, ex.Message));

            // Map common framework exceptions to HTTP status codes
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

                // Extract error code and retryable flag from structured exceptions
                var originalException = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
                string code;
                bool retryable;

                if (originalException is BaseApiException apiException)
                {
                    code = apiException.ErrorCode.ToString();
                    retryable = apiException.IsRetryable;

                    // Add the numeric error code for clients that prefer numbers
                    details.Extensions["errorCode"] = (int)apiException.ErrorCode;

                    // Add any additional metadata from the exception
                    var metadata = apiException.GetMetadata();
                    foreach (var kvp in metadata)
                    {
                        details.Extensions[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    // Fallback for non-structured exceptions
                    code = status switch
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

                    retryable = status is StatusCodes.Status429TooManyRequests or >= 500;
                }

                details.Extensions["code"] = code;
                details.Extensions["retryable"] = retryable;
                details.Extensions["correlationId"] = correlationId;

                if (status >= 500)
                {
                    logger.LogError(
                        "ProblemDetails 5xx. Status={Status}, Title={Title}, Path={Path}, CorrelationId={CorrelationId}",
                        status,
                        title,
                        path,
                        correlationId);
                }
                else
                {
                    logger.LogWarning(
                        "ProblemDetails 4xx. Status={Status}, Title={Title}, Path={Path}, CorrelationId={CorrelationId}",
                        status,
                        title,
                        path,
                        correlationId);
                }
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiProblemDetails(this IApplicationBuilder app)
        => app.UseProblemDetails();
}
