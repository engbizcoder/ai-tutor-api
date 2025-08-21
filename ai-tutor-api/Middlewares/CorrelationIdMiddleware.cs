namespace Ai.Tutor.Api.Middlewares;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var correlationId = ResolveCorrelationId(context);

        // Expose to downstream and response
        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(
            () =>
        {
            context.Response.Headers.Append(HeaderName, correlationId);

            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values))
        {
            var incoming = values.ToString();
            if (!string.IsNullOrWhiteSpace(incoming))
            {
                return incoming;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
