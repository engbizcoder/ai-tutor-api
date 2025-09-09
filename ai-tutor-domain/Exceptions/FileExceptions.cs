namespace Ai.Tutor.Domain.Exceptions;

using Ai.Tutor.Contracts.Enums;

public class FileNotFoundException : BaseApiException
{
    public FileNotFoundException()
    {
    }

    public FileNotFoundException(string message)
        : base(message)
    {
    }

    public FileNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.NotFound;

    public override int StatusCode => 404;
}

public class AttachmentNotFoundException : BaseApiException
{
    public AttachmentNotFoundException()
    {
    }

    public AttachmentNotFoundException(string message)
        : base(message)
    {
    }

    public AttachmentNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.NotFound;

    public override int StatusCode => 404;
}

public class ReferenceNotFoundException : BaseApiException
{
    public ReferenceNotFoundException()
    {
    }

    public ReferenceNotFoundException(string message)
        : base(message)
    {
    }

    public ReferenceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override ApiErrorCode ErrorCode => ApiErrorCode.NotFound;

    public override int StatusCode => 404;
}
