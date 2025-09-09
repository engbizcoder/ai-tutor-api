namespace Ai.Tutor.Contracts.DTOs;

public sealed class CreateFileRequest
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public int? Pages { get; init; }
}
