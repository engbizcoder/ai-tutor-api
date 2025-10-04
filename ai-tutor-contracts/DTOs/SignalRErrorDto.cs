namespace Ai.Tutor.Contracts.DTOs;

using System;
using System.Collections.Generic;

public sealed class SignalRErrorDto
{
    public string Code { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public bool Retryable { get; init; }

    public IDictionary<string, object>? Metadata { get; init; }
}
