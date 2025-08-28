namespace Ai.Tutor.Api.Services;

using Microsoft.AspNetCore.Builder;
using Middlewares;

public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
