namespace Ai.Tutor.Services.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service responsible for purging organizations that have exceeded their retention period.
/// </summary>
public sealed class OrgPurgeBackgroundService(
    IServiceProvider serviceProvider,
    IOrgDeletionService orgDeletionService,
    ILogger<OrgPurgeBackgroundService> logger) : BackgroundService
{
    /// <summary>
    /// Processes organizations ready for purge with safety checks and reporting.
    /// </summary>
    public async Task ProcessOrgPurgeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting organization purge process");

            var orgsReadyForPurge = await orgDeletionService.GetOrgsReadyForPurgeAsync(cancellationToken);

            if (orgsReadyForPurge.Count == 0)
            {
                logger.LogInformation("No organizations ready for purge");
                return;
            }

            logger.LogInformation(
                "Found {Count} organizations ready for purge",
                orgsReadyForPurge.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var orgId in orgsReadyForPurge)
            {
                try
                {
                    logger.LogInformation("Purging organization {OrgId}", orgId);

                    await orgDeletionService.HardDeleteOrgAsync(orgId, cancellationToken);

                    successCount++;
                    logger.LogInformation("Successfully purged organization {OrgId}", orgId);
                }
                catch (Exception ex)
                {
                    failureCount++;
                    logger.LogError(ex, "Failed to purge organization {OrgId}", orgId);

                    // Continue with other organizations even if one fails
                }
            }

            logger.LogInformation(
                "Organization purge process completed. Success: {SuccessCount}, Failures: {FailureCount}",
                successCount,
                failureCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during organization purge process");
            throw;
        }
    }

    /// <summary>
    /// Performs a dry run to report what would be purged without actually deleting anything.
    /// </summary>
    public async Task<OrgPurgeReport> GeneratePurgeReportAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var orgsReadyForPurge = await orgDeletionService.GetOrgsReadyForPurgeAsync(cancellationToken);

            return new OrgPurgeReport
            {
                GeneratedAt = DateTime.UtcNow,
                OrganizationsReadyForPurge = orgsReadyForPurge,
                TotalCount = orgsReadyForPurge.Count,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate purge report");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await this.ProcessOrgPurgeAsync(stoppingToken);

                // Wait 24 hours before next purge cycle
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in background purge service, will retry in 1 hour");

                // Wait 1 hour before retrying on error
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}

/// <summary>
/// Report containing information about organizations ready for purge.
/// </summary>
public sealed class OrgPurgeReport
{
    public DateTime GeneratedAt { get; set; }

    public List<Guid> OrganizationsReadyForPurge { get; set; } = [];

    public int TotalCount { get; set; }
}
