namespace Ai.Tutor.Domain.Exceptions;

using System.Collections.Generic;
using Ai.Tutor.Contracts.Enums;

/// <summary>
/// Base exception class that carries structured error metadata for API responses.
/// </summary>
public abstract class BaseApiException : Exception
{
    protected BaseApiException()
    {
    }

    protected BaseApiException(string message)
        : base(message)
    {
    }

    protected BaseApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Gets the stable error code for this exception type.
    /// </summary>
    public abstract ApiErrorCode ErrorCode { get; }

    /// <summary>
    /// Gets whether this error is retryable by the client.
    /// </summary>
    public virtual bool IsRetryable => false;

    /// <summary>
    /// Gets additional metadata that should be included in the error response.
    /// </summary>
    public virtual Dictionary<string, object> GetMetadata()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Gets the HTTP status code that should be returned for this exception.
    /// </summary>
    public abstract int StatusCode { get; }
}
