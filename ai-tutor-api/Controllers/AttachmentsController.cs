namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Exceptions;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Features.Attachments;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages message attachments that link chat messages to stored files.
///
/// Use this controller to:
/// - Create an attachment for a message by referencing an existing file id.
/// - List attachments for a given message.
/// - Retrieve a single attachment by id (scoped to message/thread/org).
///
/// Tenancy: Route parameters enforce context (<c>orgId</c>, <c>threadId</c>, <c>messageId</c>). The repository and services validate associations.
/// </summary>
[ApiController]
[Route("api/orgs/{orgId:guid}/threads/{threadId:guid}/messages/{messageId:guid}/attachments")]
public sealed class AttachmentsController(
    IMediator mediator,
    IAttachmentRepository attachmentsRepo) : ControllerBase
{
    /// <summary>
    /// Gets an attachment by identifier within the specified message/thread/org.
    /// When to use: After creating an attachment or when you have an attachment id and need its details.
    /// Why: Provides direct lookup and validation that the attachment belongs to the route context.
    /// Returns 404 if not found or does not belong to the specified message.
    /// </summary>
    [HttpGet("{attachmentId:guid}", Name = "GetAttachmentById")]
    public async Task<ActionResult<AttachmentDto>> GetByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromRoute] Guid messageId,
        [FromRoute] Guid attachmentId,
        CancellationToken ct = default)
    {
        var entity = await attachmentsRepo.GetByIdAsync(attachmentId, ct);
        if (entity is null || entity.MessageId != messageId)
        {
            throw new AttachmentNotFoundException($"Attachment {attachmentId} not found for message {messageId}");
        }

        return this.Ok(MapToDto(entity));
    }

    /// <summary>
    /// Lists all attachments that belong to a specific message.
    ///
    /// When to use: To display all files linked to a message.
    /// Why: Returns a simple, ordered collection (by creation time) of attachments for the message.
    /// </summary>
    [HttpGet(Name = "ListAttachments")]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> ListAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromRoute] Guid messageId,
        CancellationToken ct = default)
    {
        var items = await mediator.Send(new ListAttachmentsRequest { MessageId = messageId }, ct);
        var dtos = items.Select(MapToDto).ToList();
        return this.Ok(dtos);
    }

    /// <summary>
    /// Creates an attachment linking a message to an existing file.
    ///
    /// When to use: After uploading a file via <c>FilesController</c>, use this to attach it to a message.
    /// Why: Establishes a typed association (<c>AttachmentType</c>) and returns the created attachment.
    /// Requires: A valid <c>FileId</c> that belongs to the same organization.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AttachmentDto>> CreateAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromRoute] Guid messageId,
        [FromBody] Contracts.DTOs.CreateAttachmentRequest req,
        CancellationToken ct)
    {
        var created = await mediator.Send(
            new Ai.Tutor.Services.Features.Attachments.CreateAttachmentRequest
        {
            OrgId = orgId,
            MessageId = messageId,
            FileId = req.FileId,
            Type = (Ai.Tutor.Domain.Enums.AttachmentType)req.Type,
        },
            ct);

        var dto = MapToDto(created);
        return this.CreatedAtRoute("GetAttachmentById", new { orgId, threadId, messageId, attachmentId = dto.Id }, dto);
    }

    private static AttachmentDto MapToDto(Attachment x) => new()
    {
        Id = x.Id,
        MessageId = x.MessageId,
        FileId = x.FileId,
        Type = (Ai.Tutor.Contracts.Enums.AttachmentType)x.Type,
        CreatedAt = x.CreatedAt,
        File = null,
    };
}
