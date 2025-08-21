namespace Ai.Tutor.Api.Middlewares;

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public sealed class ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
{
    private static readonly Action<ILogger, Exception?> UnhandledException =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2001, nameof(UnhandledException)),
            "Unhandled exception");

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var id)
                ? id?.ToString()
                : null;

            using (logger.BeginScope(new Dictionary<string, object>
            {
                [CorrelationIdMiddleware.ItemKey] = correlationId ?? string.Empty,
            }))
            {
                UnhandledException(logger, ex);
            }

            throw;
        }
    }
}
