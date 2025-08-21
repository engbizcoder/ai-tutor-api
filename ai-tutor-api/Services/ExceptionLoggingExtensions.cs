namespace Ai.Tutor.Api.Services;

using Ai.Tutor.Api.Middlewares;
using Microsoft.AspNetCore.Builder;

public static class ExceptionLoggingExtensions
{
    public static IApplicationBuilder UseExceptionLogging(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionLoggingMiddleware>();
}
