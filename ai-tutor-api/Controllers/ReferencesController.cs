namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Features.References;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages references that link threads/messages to external resources (URLs) or stored files.
///
/// Use this controller to:
/// - Create a reference for a thread (optionally tied to a message).
/// - List references within a thread (cursor-based pagination).
/// - Retrieve a single reference by id.
///
/// Tenancy: All operations are scoped by <c>orgId</c> and <c>threadId</c>. File-backed references validate file ownership by organization.
/// </summary>
[ApiController]
[Route("api/orgs/{orgId:guid}/threads/{threadId:guid}/references")]
public sealed class ReferencesController(
    IMediator mediator,
    IReferenceRepository references) : ControllerBase
{
    /// <summary>
    /// Gets a reference by identifier within the specified organization and thread.
    /// When to use: After creating a reference or when you have a reference id and need its details.
    /// Why: Provides direct lookup and validates the reference belongs to the route's thread and organization.
    /// Returns 404 if the reference is not found or does not belong to the thread/org context.
    /// </summary>
    [HttpGet("{referenceId:guid}", Name = "GetReferenceById")]
    public async Task<ActionResult<ReferenceDto>> GetByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromRoute] Guid referenceId,
        CancellationToken ct = default)
    {
        var entity = await references.GetByIdAsync(referenceId, orgId, ct);
        if (entity is null || entity.ThreadId != threadId)
        {
            throw new Ai.Tutor.Domain.Exceptions.ReferenceNotFoundException($"Reference {referenceId} not found for thread {threadId}");
        }

        return this.Ok(MapToDto(entity));
    }

    /// <summary>
    /// Lists references within a thread using cursor-based pagination.
    /// When to use: To display citations/resources attached to a thread, optionally tied to specific messages.
    /// Why: Supports efficient paging for long-running threads with many references.
    /// </summary>
    [HttpGet(Name = "ListReferences")]
    public async Task<ActionResult<PagedReferencesResponse>> ListAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new ListReferencesRequest
        {
            OrgId = orgId,
            ThreadId = threadId,
            PageSize = pageSize,
            Cursor = cursor,
        },
            ct);

        var response = new PagedReferencesResponse
        {
            Items = result.Items.Select(MapToDto).ToList(),
            NextCursor = result.NextCursor,
            HasMore = result.NextCursor != null,
        };
        return this.Ok(response);
    }

    /// <summary>
    /// Creates a reference for a thread (optionally tied to a specific message), pointing to a URL or a stored file.
    /// When to use: To associate learning materials, web links, or uploaded documents with a conversation thread.
    /// Why: Enables structured citations and resource tracking. Either <c>Url</c> or <c>FileId</c> must be provided.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReferenceDto>> CreateAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromBody] Contracts.DTOs.CreateReferenceRequest req,
        CancellationToken ct)
    {
        var created = await mediator.Send(
            new Tutor.Services.Features.References.CreateReferenceRequest
        {
            OrgId = orgId,
            ThreadId = threadId,
            MessageId = req.MessageId,
            Type = (Ai.Tutor.Domain.Enums.ReferenceType)req.Type,
            Title = req.Title,
            Url = req.Url,
            FileId = req.FileId,
            PageNumber = req.PageNumber,
            PreviewImgUrl = req.PreviewImgUrl,
        },
            ct);

        var dto = MapToDto(created);
        return this.CreatedAtRoute("GetReferenceById", new { orgId, threadId, referenceId = dto.Id }, dto);
    }

    private static ReferenceDto MapToDto(Reference x) => new()
    {
        Id = x.Id,
        ThreadId = x.ThreadId,
        MessageId = x.MessageId,
        Type = (Ai.Tutor.Contracts.Enums.ReferenceType)x.Type,
        Title = x.Title,
        Url = x.Url,
        FileId = x.FileId,
        PageNumber = x.PageNumber,
        PreviewImgUrl = x.PreviewImgUrl,
        CreatedAt = x.CreatedAt,
        File = null,
    };
}
