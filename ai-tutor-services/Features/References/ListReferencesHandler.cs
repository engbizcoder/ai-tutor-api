namespace Ai.Tutor.Services.Features.References;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Exceptions;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Microsoft.Extensions.Logging;

public sealed class ListReferencesHandler(
    IReferenceRepository references,
    IThreadRepository threads,
    ILogger<ListReferencesHandler> logger) : IRequestHandler<ListReferencesRequest, (IReadOnlyList<Reference> Items, string? NextCursor)>
{
    public async Task<(IReadOnlyList<Reference> Items, string? NextCursor)> Handle(ListReferencesRequest request, CancellationToken ct = default)
    {
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        logger.LogInformation("Listing references for thread {ThreadId} in org {OrgId} with page size {PageSize}", request.ThreadId, request.OrgId, pageSize);

        // Validate thread exists and belongs to org
        _ = await threads.GetAsync(request.ThreadId, request.OrgId, ct) 
            ?? throw new ThreadNotFoundException($"Thread {request.ThreadId} not found in org {request.OrgId}");

        var result = await references.ListByThreadPagedAsync(request.ThreadId, request.OrgId, pageSize, request.Cursor, ct);
        logger.LogInformation("Listed {Count} references", result.Items.Count);
        return result;
    }
}
