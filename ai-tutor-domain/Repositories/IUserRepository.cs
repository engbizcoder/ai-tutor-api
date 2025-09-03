namespace Ai.Tutor.Domain.Repositories;

using Entities;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);

    Task DeleteAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets users who have the specified org as their primary org.
    /// </summary>
    Task<List<User>> GetUsersByPrimaryOrgAsync(Guid orgId, CancellationToken ct = default);

    /// <summary>
    /// Updates the primary org for a user.
    /// </summary>
    Task UpdatePrimaryOrgAsync(Guid userId, Guid newPrimaryOrgId, CancellationToken ct = default);

    /// <summary>
    /// Creates a personal org for a user if they don't have one.
    /// </summary>
    Task<Org> EnsurePersonalOrgAsync(Guid userId, string userEmail, CancellationToken ct = default);
}
