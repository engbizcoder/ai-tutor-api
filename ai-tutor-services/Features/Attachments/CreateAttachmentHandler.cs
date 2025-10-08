namespace Ai.Tutor.Services.Features.Attachments;

using Ai.Tutor.Domain.Exceptions;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed class CreateAttachmentHandler(
    IAttachmentRepository attachments,
    IFileRepository files,
    IUnitOfWork uow,
    ILogger<CreateAttachmentHandler> logger) : IRequestHandler<CreateAttachmentRequest, Attachment>
{
    public async Task<Attachment> Handle(CreateAttachmentRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating attachment for message {MessageId} to file {FileId} in org {OrgId}", request.MessageId, request.FileId, request.OrgId);

        // Validate file exists and belongs to org
        _ = await files.GetByIdAsync(request.FileId, request.OrgId, ct)
            ?? throw new FileNotFoundException($"File {request.FileId} not found in org {request.OrgId}");

        var entity = new Attachment
        {
            MessageId = request.MessageId,
            FileId = request.FileId,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
        };

        Attachment created = entity;
        await uow.ExecuteInTransactionAsync(
            async ctk =>
        {
            created = await attachments.AddAsync(entity, ctk);
        },
            ct);

        logger.LogInformation("Attachment {AttachmentId} created successfully", created.Id);
        return created;
    }
}
