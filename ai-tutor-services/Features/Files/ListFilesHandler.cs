namespace Ai.Tutor.Services.Features.Files;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Microsoft.Extensions.Logging;

public sealed class ListFilesHandler(
    IFileRepository files,
    ILogger<ListFilesHandler> logger) : IRequestHandler<ListFilesRequest, (IReadOnlyList<StoredFile> Items, string? NextCursor)>
{
    public async Task<(IReadOnlyList<StoredFile> Items, string? NextCursor)> Handle(ListFilesRequest request, CancellationToken ct = default)
    {
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        logger.LogInformation("Listing files for owner {OwnerUserId} in org {OrgId} with page size {PageSize}", request.OwnerUserId, request.OrgId, pageSize);
        var result = await files.ListByOwnerPagedAsync(request.OwnerUserId, request.OrgId, pageSize, request.Cursor, ct);
        logger.LogInformation("Listed {Count} files", result.Items.Count);
        return result;
    }
}
