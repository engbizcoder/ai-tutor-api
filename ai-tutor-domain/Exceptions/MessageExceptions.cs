namespace Ai.Tutor.Domain.Exceptions;

using Ai.Tutor.Contracts.Enums;

public class MessageNotFoundException : BaseApiException
{
    public MessageNotFoundException()
    {
    }

    public MessageNotFoundException(string message)
        : base(message)
    {
    }

    public MessageNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.MessageNotFound;

    public override int StatusCode => 404;
}

public class DuplicateMessageException : BaseApiException
{
    public DuplicateMessageException(string message)
        : base(message)
    {
    }

    public DuplicateMessageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DuplicateMessageException()
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.DuplicateMessage;

    public override int StatusCode => 409;
}

public class InvalidMessageContentException : BaseApiException
{
    public InvalidMessageContentException(string message)
        : base(message)
    {
    }

    public InvalidMessageContentException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public InvalidMessageContentException()
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.InvalidMessageContent;

    public override int StatusCode => 400;
}

public class ThreadNotAccessibleException : BaseApiException
{
    public ThreadNotAccessibleException()
    {
    }

    public ThreadNotAccessibleException(string message)
        : base(message)
    {
    }

    public ThreadNotAccessibleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.ThreadAccessDenied;

    public override int StatusCode => 403;
}

public class ThreadNotFoundException : BaseApiException
{
    public ThreadNotFoundException()
    {
    }

    public ThreadNotFoundException(string message)
        : base(message)
    {
    }

    public ThreadNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.ThreadNotFound;

    public override int StatusCode => 404;
}
