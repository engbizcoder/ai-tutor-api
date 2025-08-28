namespace Ai.Tutor.Services.Features.Folders;

using Mediation;

public sealed class DeleteFolderRequest : IRequest
{
    public Guid OrgId { get; init; }

    public Guid FolderId { get; init; }
}
