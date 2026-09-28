namespace HMS.Modules.Finance.Application.DTOs;

public sealed record UtilityBillDto(
    Guid Uid,
    Guid PropertyUid,
    Guid UtilityTypeUid,
    string UtilityTypeName,
    string? UnitOfMeasure,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal? PreviousReading,
    decimal? CurrentReading,
    decimal? UnitsUsed,
    decimal Amount,
    string Currency,
    DateOnly? DueDate,
    DateTimeOffset? PaidAt,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreationDate);
