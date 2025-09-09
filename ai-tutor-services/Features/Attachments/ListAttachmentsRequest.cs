namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Services.Mediation;

public sealed class ListAttachmentsRequest : IRequest<IReadOnlyList<Attachment>>
{
    public Guid MessageId { get; init; }
}
