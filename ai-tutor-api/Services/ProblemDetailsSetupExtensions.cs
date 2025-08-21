namespace Ai.Tutor.Api.Services;

using System.ComponentModel.DataAnnotations;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ProblemDetailsSetupExtensions
{
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
        });

        return services;
    }

    public static IApplicationBuilder UseApiProblemDetails(this IApplicationBuilder app)
        => app.UseProblemDetails();
}
