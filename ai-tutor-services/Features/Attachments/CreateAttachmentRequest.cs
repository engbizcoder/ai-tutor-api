namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Services.Mediation;

public sealed class CreateAttachmentRequest : IRequest<Attachment>
{
    public Guid OrgId { get; init; }

    public Guid MessageId { get; init; }

    public Guid FileId { get; init; }

    public AttachmentType Type { get; init; }
}
