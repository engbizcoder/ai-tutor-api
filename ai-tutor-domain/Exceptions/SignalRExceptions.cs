namespace Ai.Tutor.Domain.Exceptions;

using System;

public class ThreadAccessDeniedException : UnauthorizedAccessException
{
    public ThreadAccessDeniedException()
    {
    }

    public ThreadAccessDeniedException(string message)
        : base(message)
    {
    }

    public ThreadAccessDeniedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class InvalidThreadException : ArgumentException
{
    public InvalidThreadException()
    {
    }

    public InvalidThreadException(string message)
        : base(message)
    {
    }

    public InvalidThreadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class SignalRConnectionException : InvalidOperationException
{
    public SignalRConnectionException()
    {
    }

    public SignalRConnectionException(string message)
        : base(message)
    {
    }

    public SignalRConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
