namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class ListAttachmentsRequest : IRequest<IReadOnlyList<Attachment>>
{
    public Guid MessageId { get; init; }
}
