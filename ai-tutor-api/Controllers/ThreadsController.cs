namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Features.Threads;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orgs/{orgId:guid}/threads")]
public sealed class ThreadsController(IMediator mediator, IThreadRepository threads) : ControllerBase
{
    [HttpDelete("{threadId:guid}")]
    public async Task<IActionResult> DeleteThreadAsync([FromRoute] Guid orgId, [FromRoute] Guid threadId, CancellationToken ct)
    {
        var existing = await threads.GetAsync(threadId, orgId, ct);
        if (existing is null)
        {
            return this.NotFound();
        }

        await mediator.Send(new DeleteThreadRequest { OrgId = orgId, ThreadId = threadId }, ct);
        return this.NoContent();
    }
}
