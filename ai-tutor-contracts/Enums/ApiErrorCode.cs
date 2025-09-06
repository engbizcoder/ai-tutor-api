namespace Ai.Tutor.Contracts.Enums;

/// <summary>
/// Standardized API error codes that clients can use to handle specific error scenarios.
/// These codes are stable and won't change between API versions.
/// </summary>
public enum ApiErrorCode
{
    // General errors (1000-1999)
    BadRequest = 1000,
    Unauthorized = 1001,
    Forbidden = 1002,
    NotFound = 1003,
    Conflict = 1004,
    InternalError = 1005,
    SemanticError = 1006,
    RateLimit = 1007,

    // Message-specific errors (2000-2099)
    MessageNotFound = 2000,
    DuplicateMessage = 2001,
    InvalidMessageContent = 2002,

    // Thread-specific errors (2100-2199)
    ThreadNotFound = 2100,
    ThreadAccessDenied = 2101,

    // Folder-specific errors (2200-2299)
    FolderNotFound = 2200,
    FolderAccessDenied = 2201,

    // Organization-specific errors (2300-2399)
    OrganizationNotFound = 2300,
    OrganizationAccessDenied = 2301,

    // User-specific errors (2400-2499)
    UserNotFound = 2400,
    UserAccessDenied = 2401,

    // Validation errors (3000-3099)
    ValidationFailed = 3000,
    RequiredFieldMissing = 3001,
    InvalidFieldFormat = 3002,
    FieldTooLong = 3003,
    FieldTooShort = 3004,
}
