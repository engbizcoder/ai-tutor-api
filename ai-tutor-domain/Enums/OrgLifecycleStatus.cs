namespace Ai.Tutor.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of an organization.
/// Active → Disabled → Deleted → Purged (after retention period).
/// </summary>
public enum OrgLifecycleStatus
{
    /// <summary>
    /// Organization is fully operational with read/write access.
    /// </summary>
    Active,

    /// <summary>
    /// Organization is read-only, no new writes allowed.
    /// </summary>
    Disabled,

    /// <summary>
    /// Organization is hidden and queued for purge after retention period.
    /// </summary>
    Deleted,

    /// <summary>
    /// Organization data has been permanently purged.
    /// </summary>
    Purged,
}
