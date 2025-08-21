namespace Ai.Tutor.Domain.Repositories;

using Ai.Tutor.Domain.Entities;

public interface IOrgMemberRepository
{
    Task<bool> IsMemberAsync(Guid orgId, Guid userId, CancellationToken ct = default);

    Task AddAsync(OrgMember member, CancellationToken ct = default);

    Task DeleteByOrgAsync(Guid orgId, CancellationToken ct = default);

    Task DeleteByUserAsync(Guid userId, CancellationToken ct = default);
}
