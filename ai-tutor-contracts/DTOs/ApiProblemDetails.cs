namespace Ai.Tutor.Contracts.DTOs;

using System.Text.Json.Serialization;

public sealed class ApiProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public int? Status { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    // Stable application error code (e.g., FOLDER_NAME_TAKEN, THREAD_NOT_FOUND)
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    // Validation errors (for 400/422)
    [JsonPropertyName("errors")]
    public IDictionary<string, string[]>? Errors { get; init; }

    // Indicates whether client MAY retry (e.g., 429/5xx transient)
    [JsonPropertyName("retryable")]
    public bool? Retryable { get; init; }

    // Additional safe metadata
    [JsonPropertyName("meta")]
    public IDictionary<string, object?>? Meta { get; init; }
}