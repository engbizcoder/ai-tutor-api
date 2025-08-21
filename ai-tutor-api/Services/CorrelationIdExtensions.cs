namespace Ai.Tutor.Api.Services;

using Ai.Tutor.Api.Middlewares;
using Microsoft.AspNetCore.Builder;

public static class CorrelationIdExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
