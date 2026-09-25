namespace HMS.Modules.Finance.Api.Contracts;

public sealed record CreateUtilityBillRequest(
    Guid UtilityTypeUid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal Amount,
    decimal? PreviousReading = null,
    decimal? CurrentReading = null,
    decimal? UnitsUsed = null,
    string Currency = "LKR",
    DateOnly? DueDate = null,
    DateTimeOffset? PaidAt = null,
    string? ReferenceNumber = null,
    string? Notes = null);

public sealed record UpdateUtilityBillRequest(
    Guid UtilityTypeUid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal Amount,
    decimal? PreviousReading = null,
    decimal? CurrentReading = null,
    decimal? UnitsUsed = null,
    string Currency = "LKR",
    DateOnly? DueDate = null,
    DateTimeOffset? PaidAt = null,
    string? ReferenceNumber = null,
    string? Notes = null);    
