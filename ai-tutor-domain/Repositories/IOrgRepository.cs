namespace Ai.Tutor.Domain.Repositories;

using Entities;

public interface IOrgRepository
{
    Task<Org?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Org> AddAsync(Org org, CancellationToken ct = default);

    Task DeleteAsync(Guid orgId, CancellationToken ct = default);
}
