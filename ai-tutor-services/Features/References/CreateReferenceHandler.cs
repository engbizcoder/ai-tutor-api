namespace Ai.Tutor.Services.Features.References;

using Ai.Tutor.Domain.Exceptions;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed class CreateReferenceHandler(
    IReferenceRepository references,
    IThreadRepository threads,
    IFileRepository files,
    IUnitOfWork uow,
    ILogger<CreateReferenceHandler> logger) : IRequestHandler<CreateReferenceRequest, Reference>
{
    public async Task<Reference> Handle(CreateReferenceRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Creating reference in thread {ThreadId} (org {OrgId})", request.ThreadId, request.OrgId);

        // Validate thread exists and belongs to org
        _ = await threads.GetAsync(request.ThreadId, request.OrgId, ct)
            ?? throw new ThreadNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}");

        // Validate file if provided belongs to org
        if (request.FileId.HasValue)
        {
            _ = await files.GetByIdAsync(request.FileId.Value, request.OrgId, ct)
                ?? throw new FileNotFoundException($"File {request.FileId} not found in org {request.OrgId}");
        }

        var entity = new Reference
        {
            ThreadId = request.ThreadId,
            MessageId = request.MessageId,
            Type = request.Type,
            Title = request.Title,
            Url = request.Url,
            FileId = request.FileId,
            PageNumber = request.PageNumber,
            PreviewImgUrl = request.PreviewImgUrl,
            CreatedAt = DateTime.UtcNow,
        };

        // Ensure either Url or FileId
        entity.Validate();

        Reference created = entity;
        await uow.ExecuteInTransactionAsync(
            async ctk =>
        {
            created = await references.AddAsync(entity, request.OrgId, ctk);
        },
            ct);

        logger.LogInformation("Reference {ReferenceId} created", created.Id);
        return created;
    }
}
