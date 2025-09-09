namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Contracts.DTOs;
using Ai.Tutor.Domain.Entities;
using Ai.Tutor.Domain.Exceptions;
using Ai.Tutor.Domain.Repositories;
using Ai.Tutor.Services.Features.Files;
using Ai.Tutor.Services.Mediation;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Manages organization-scoped stored files (metadata + content stream).
///
/// Use this controller to:
/// - Upload a new file (multipart form) and create a <see cref="Contracts.DTOs.FileDto"/> record.
/// - List files for a specific owner within an organization (cursor-based pagination).
/// - Retrieve a single file's metadata by id.
/// - Download the raw file content by id.
///
/// Tenancy: All operations are scoped by <c>orgId</c>. The list endpoint also requires the <c>ownerUserId</c> to filter files per user.
/// </summary>
[ApiController]
[Route("api/orgs/{orgId:guid}/files")]
public sealed class FilesController(
    IMediator mediator,
    IFileRepository files,
    IFileStorageAdapter storage) : ControllerBase
{
    /// <summary>
    /// Gets file metadata by identifier within the specified organization.
    ///
    /// When to use: After uploading a file or when you have a file id and need its metadata (name, size, content type, URL, etc.).
    /// Why: Provides a direct lookup for client navigation and confirmation without downloading the content.
    /// Returns 404 if the file does not belong to the provided <c>orgId</c>.
    /// </summary>
    [HttpGet("{fileId:guid}", Name = "GetFileById")]
    public async Task<ActionResult<FileDto>> GetByIdAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid fileId,
        CancellationToken ct = default)
    {
        var entity = await files.GetByIdAsync(fileId, orgId, ct);
        if (entity is null)
        {
            throw new FileNotFoundException($"File {fileId} not found in org {orgId}");
        }

        return this.Ok(MapToDto(entity));
    }

    /// <summary>
    /// Lists files owned by a specific user within an organization using cursor-based pagination.
    ///
    /// When to use: To show the authenticated user's library or when building pickers/attachment dialogs.
    /// Why: Supports efficient paging via <c>pageSize</c> and <c>cursor</c> for large libraries.
    /// </summary>
    [HttpGet(Name = "ListFiles")]
    public async Task<ActionResult<PagedFilesResponse>> ListAsync(
        [FromRoute] Guid orgId,
        [FromQuery] Guid ownerUserId,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new ListFilesRequest
        {
            OrgId = orgId,
            OwnerUserId = ownerUserId,
            PageSize = pageSize,
            Cursor = cursor,
        },
            ct);

        var response = new PagedFilesResponse
        {
            Items = result.Items.Select(MapToDto).ToList(),
            NextCursor = result.NextCursor,
            HasMore = result.NextCursor != null,
        };
        return this.Ok(response);
    }

    /// <summary>
    /// Uploads a file (multipart/form-data) and creates a stored file record scoped to the organization and owner.
    ///
    /// When to use: For user-driven uploads from the client (e.g., attaching a document or adding to the library).
    /// Why: Handles the full flow: stream upload to storage, metadata persistence, and returns the created file DTO.
    /// Requires: <c>ownerUserId</c> query param; multipart form fields include one <c>file</c> part and optional metadata
    /// (<c>FileName</c>, <c>ContentType</c>, <c>Pages</c>).
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(524_288_000)] // 500MB safeguard
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileDto>> UploadAsync(
        [FromRoute] Guid orgId,
        [FromQuery] Guid ownerUserId,
        [FromForm] Contracts.DTOs.CreateFileRequest meta,
        [FromForm] IFormFile? file,
        CancellationToken ct)
    {
        if (file is null || file.Length <= 0)
        {
            return this.BadRequest(
                new ProblemDetails
            {
                Title = "Invalid file",
                Detail = "No file was uploaded or the file is empty.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        await using var stream = file.OpenReadStream();
        var created = await mediator.Send(
            new Tutor.Services.Features.Files.CreateFileRequest
        {
            OrgId = orgId,
            OwnerUserId = ownerUserId,
            FileName = string.IsNullOrWhiteSpace(meta.FileName) ? file.FileName : meta.FileName,
            ContentType = string.IsNullOrWhiteSpace(meta.ContentType) ? (file.ContentType ?? "application/octet-stream") : meta.ContentType,
            FileStream = stream,
            SizeBytes = file.Length,
            Pages = meta.Pages,
        },
            ct);

        var dto = MapToDto(created);
        return this.CreatedAtRoute("GetFileById", new { orgId, fileId = dto.Id }, dto);
    }

    /// <summary>
    /// Downloads the raw file content stream by identifier within the specified organization.
    ///
    /// When to use: To allow clients to fetch the actual bytes of a stored file (e.g., open or save locally).
    /// Why: Streams content with correct content type and suggested filename.
    /// Returns 404 if the file does not belong to the provided <c>orgId</c>.
    /// </summary>
    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> DownloadAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid fileId,
        CancellationToken ct)
    {
        var file = await files.GetByIdAsync(fileId, orgId, ct);
        if (file is null)
        {
            throw new FileNotFoundException($"File {fileId} not found in org {orgId}");
        }

        var stream = await storage.DownloadAsync(file.StorageKey, ct);
        return this.File(stream, file.ContentType, file.FileName);
    }

    private static FileDto MapToDto(StoredFile x) => new()
    {
        Id = x.Id,
        OwnerUserId = x.OwnerUserId,
        FileName = x.FileName,
        ContentType = x.ContentType,
        StorageUrl = x.StorageUrl,
        SizeBytes = x.SizeBytes,
        Pages = x.Pages,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}
