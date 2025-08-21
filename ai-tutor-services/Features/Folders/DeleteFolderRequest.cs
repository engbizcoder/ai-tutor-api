namespace Ai.Tutor.Services.Features.Folders;

using Ai.Tutor.Services.Mediation;

public sealed class DeleteFolderRequest : IRequest
{
    public Guid OrgId { get; init; }

    public Guid FolderId { get; init; }
}
