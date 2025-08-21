namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IOrgRepository
{
    Task<Org?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<Org> AddAsync(Org org, CancellationToken ct = default);

    Task DeleteAsync(Guid orgId, CancellationToken ct = default);
}
