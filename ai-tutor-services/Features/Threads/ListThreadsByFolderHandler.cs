namespace Ai.Tutor.Services.Features.Threads;

using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;

public sealed class ListThreadsByFolderHandler(IThreadRepository threads) : IRequestHandler<ListThreadsByFolderRequest, List<ChatThread>>
{
    public async Task<List<ChatThread>> Handle(ListThreadsByFolderRequest request, CancellationToken ct = default)
        => await threads.ListByFolderAsync(request.OrgId, request.FolderId, ct);
}
