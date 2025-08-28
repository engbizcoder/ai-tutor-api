namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Domain.Repositories;
using Mediation;

public sealed class ListThreadsByFolderHandler(IThreadRepository threads) : IRequestHandler<ListThreadsByFolderRequest, List<ChatThread>>
{
    public async Task<List<ChatThread>> Handle(ListThreadsByFolderRequest request, CancellationToken ct = default)
        => await threads.ListByFolderAsync(request.OrgId, request.FolderId, ct);
}
