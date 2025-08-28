namespace Ai.Tutor.Services.Features.Threads;

using Domain.Entities;
using Mediation;

public sealed class ListThreadsByFolderRequest : IRequest<List<ChatThread>>
{
    public Guid OrgId { get; init; }

    public Guid? FolderId { get; init; }
}
