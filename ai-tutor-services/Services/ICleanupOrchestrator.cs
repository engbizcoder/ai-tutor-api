namespace Ai.Tutor.Services.Services;

using Ai.Tutor.Domain.Repositories;

/// <summary>
/// Domain-level orchestrator that coordinates cleanup activities after entity deletions.
/// </summary>
public interface ICleanupOrchestrator
{
    /// <summary>
    /// Invoked after one or more threads have been deleted. Will cleanup any orphaned files
    /// that were referenced by messages or references in those threads.
    /// </summary>
    Task OnThreadsDeletedAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);

    /// <summary>
    /// Invoked after references have been deleted. Will cleanup any orphaned files they pointed to.
    /// </summary>
    Task OnReferencesDeletedAsync(IReadOnlyCollection<Guid> threadIds, CancellationToken ct = default);

    /// <summary>
    /// Invoked after messages have been deleted. Will cleanup any orphaned files that were attached to them.
    /// </summary>
    Task OnMessagesDeletedAsync(IReadOnlyCollection<Guid> messageIds, CancellationToken ct = default);
}
