namespace Ai.Tutor.Services.Services;

public interface IOrgDeletionService
{
    Task DeleteOrgAndDataAsync(Guid orgId, CancellationToken cancellationToken = default);
}
