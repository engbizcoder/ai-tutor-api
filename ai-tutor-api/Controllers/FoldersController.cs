namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Features.Folders;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orgs/{orgId:guid}/folders")]
public sealed class FoldersController(IMediator mediator, IFolderRepository folders) : ControllerBase
{
    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> DeleteFolderAsync([FromRoute] Guid orgId, [FromRoute] Guid folderId, CancellationToken ct)
    {
        var existing = await folders.GetAsync(folderId, orgId, ct);
        if (existing is null)
        {
            return this.NotFound();
        }

        await mediator.Send(new DeleteFolderRequest { OrgId = orgId, FolderId = folderId }, ct);
        return this.NoContent();
    }
}
