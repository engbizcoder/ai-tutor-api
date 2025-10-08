namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class CreateAttachmentRequest : IRequest<Attachment>
{
    public Guid OrgId { get; init; }

    public Guid MessageId { get; init; }

    public Guid FileId { get; init; }

    public AttachmentType Type { get; init; }
}
