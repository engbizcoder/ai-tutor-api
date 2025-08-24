namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Services.Features.Folders;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orgs/{orgId:guid}/folders")]
public sealed class FoldersController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> DeleteFolderAsync([FromRoute] Guid orgId, [FromRoute] Guid folderId, CancellationToken ct)
    {
        await mediator.Send(new DeleteFolderRequest { OrgId = orgId, FolderId = folderId }, ct);
        return this.NoContent();
    }
}
