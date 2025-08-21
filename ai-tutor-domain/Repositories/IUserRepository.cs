namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<User> AddAsync(User user, CancellationToken ct = default);

    Task DeleteAsync(Guid userId, CancellationToken ct = default);
}
