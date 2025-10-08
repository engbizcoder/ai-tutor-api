namespace Ai.Tutor.Services.Features.Files;

using System.IO;
using Ai.Tutor.Services.Mediation;
using Domain.Entities;

public sealed class CreateFileRequest : IRequest<StoredFile>
{
    public Guid OrgId { get; init; }

    public Guid OwnerUserId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public Stream FileStream { get; init; } = Stream.Null;

    public long SizeBytes { get; init; }

    public string? ChecksumSha256 { get; init; }

    public int? Pages { get; init; }
}
