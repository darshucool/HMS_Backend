namespace HMS.Modules.Identity.Application.DTOs;

public sealed record PropertyAdminDto(
    Guid AdminId,
    Guid PropertyUid,
    string? Email,
    string FirstName,
    string? LastName,
    bool IsActive,
    DateTimeOffset AssignedAt);

public sealed record PropertyAdminResult<T>
{
    public bool IsSuccessful { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static PropertyAdminResult<T> Success(T data) => new()
    {
        IsSuccessful = true,
        Data = data
    };

    public static PropertyAdminResult<T> Failure(string errorCode, string errorMessage) => new()
    {
        IsSuccessful = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };
}
