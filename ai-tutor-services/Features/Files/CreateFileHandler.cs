namespace Ai.Tutor.Services.Features.Files;

using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed class CreateFileHandler(
    IFileRepository files,
    IFileStorageAdapter storage,
    IUnitOfWork uow,
    ILogger<CreateFileHandler> logger) : IRequestHandler<CreateFileRequest, StoredFile>
{
    public async Task<StoredFile> Handle(CreateFileRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Uploading file {FileName} (content-type: {ContentType}) for org {OrgId} owner {OwnerUserId}", request.FileName, request.ContentType, request.OrgId, request.OwnerUserId);

        // Deduplication by checksum if provided
        if (!string.IsNullOrWhiteSpace(request.ChecksumSha256))
        {
            var existing = await files.GetByChecksumAsync(request.ChecksumSha256, request.OrgId, ct);
            if (existing is not null)
            {
                logger.LogInformation("Found existing file {FileId} by checksum, skipping upload", existing.Id);
                return existing;
            }
        }

        string storageKey = await storage.UploadAsync(request.FileStream, request.FileName, request.ContentType, ct);
        string? storageUrl = await storage.GetPresignedUrlAsync(storageKey, TimeSpan.FromHours(1), ct);

        var entity = new StoredFile
        {
            OrgId = request.OrgId,
            OwnerUserId = request.OwnerUserId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            StorageKey = storageKey,
            StorageUrl = storageUrl,
            SizeBytes = request.SizeBytes,
            ChecksumSha256 = request.ChecksumSha256,
            Pages = request.Pages,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        StoredFile created = entity;

        await uow.ExecuteInTransactionAsync(
            async ctk =>
        {
            created = await files.AddAsync(entity, ctk);
        },
            ct);

        logger.LogInformation("File {FileId} created successfully for org {OrgId}", created.Id, created.OrgId);
        return created;
    }
}
