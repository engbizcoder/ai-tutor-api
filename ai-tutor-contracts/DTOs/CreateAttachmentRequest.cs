namespace Ai.Tutor.Contracts.DTOs;

using Enums;

public sealed class CreateAttachmentRequest
{
    public Guid FileId { get; init; }

    public AttachmentType Type { get; init; }
}
