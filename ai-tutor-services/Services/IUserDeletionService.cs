namespace Ai.Tutor.Services.Services;

public interface IUserDeletionService
{
    Task DeleteUserAndDataAsync(Guid userId, CancellationToken cancellationToken = default);
}
