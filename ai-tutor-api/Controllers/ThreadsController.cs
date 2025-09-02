namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Services.Features.Threads;
using Ai.Tutor.Services.Mediation;
using Contracts.DTOs;
using Contracts.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orgs/{orgId:guid}/threads")]
public sealed class ThreadsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ThreadDto>> CreateAsync([FromRoute] Guid orgId, [FromBody] Contracts.DTOs.CreateThreadRequest req, CancellationToken ct)
    {
        var created = await mediator.Send(
            new Ai.Tutor.Services.Features.Threads.CreateThreadRequest
        {
            OrgId = orgId,
            UserId = req.UserId,
            FolderId = req.FolderId,
            Title = req.Title,
            Status = (Domain.Enums.ChatThreadStatus)req.Status,
            SortOrder = req.SortOrder ?? 1000m,
        },
            ct);
        var dto = MapToDto(created);
        return this.CreatedAtRoute("GetThreadById", new { orgId, threadId = dto.Id }, dto);
    }

    [HttpGet("{threadId:guid}", Name="GetThreadById")]
    public async Task<ActionResult<ThreadDto>> GetByIdAsync([FromRoute] Guid orgId, [FromRoute] Guid threadId, CancellationToken ct)
    {
        var item = await mediator.Send(new GetThreadByIdRequest { OrgId = orgId, ThreadId = threadId }, ct);
        return this.Ok(MapToDto(item));
    }

    [HttpGet("by-folder")]
    public async Task<ActionResult<IReadOnlyList<ThreadDto>>> ListByFolderAsync([FromRoute] Guid orgId, [FromQuery] Guid? folderId, CancellationToken ct)
    {
        var items = await mediator.Send(new ListThreadsByFolderRequest { OrgId = orgId, FolderId = folderId }, ct);
        return this.Ok(items.Select(MapToDto).ToList());
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ThreadDto>>> ListByUserAsync([FromRoute] Guid orgId, [FromRoute] Guid userId, CancellationToken ct)
    {
        var items = await mediator.Send(new ListThreadsByUserRequest { OrgId = orgId, UserId = userId }, ct);
        return this.Ok(items.Select(MapToDto).ToList());
    }

    [HttpPatch("{threadId:guid}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid orgId, [FromRoute] Guid threadId, [FromBody] UpdateThreadRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new UpdateThreadCommand
        {
            OrgId = orgId,
            ThreadId = threadId,
            Title = req.Title,
            Status = req.Status.HasValue ? (Domain.Enums.ChatThreadStatus)req.Status.Value : null,
            NewFolderId = req.NewFolderId,
            SortOrder = req.SortOrder,
        },
            ct);
        return this.NoContent();
    }

    [HttpDelete("{threadId:guid}")]
    public async Task<IActionResult> DeleteThreadAsync([FromRoute] Guid orgId, [FromRoute] Guid threadId, CancellationToken ct)
    {
        await mediator.Send(new DeleteThreadRequest { OrgId = orgId, ThreadId = threadId }, ct);
        return this.NoContent();
    }

    private static ThreadDto MapToDto(ChatThread x) => new()
    {
        Id = x.Id,
        UserId = x.UserId,
        FolderId = x.FolderId,
        Title = x.Title,
        Status = (ThreadStatus)x.Status,
        SortOrder = x.SortOrder,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
