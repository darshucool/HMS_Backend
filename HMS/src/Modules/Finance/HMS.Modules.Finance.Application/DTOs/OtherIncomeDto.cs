namespace HMS.Modules.Finance.Application.DTOs;

public sealed record OtherIncomeDto(
    Guid Uid,
    Guid PropertyUid,
    Guid IncomeCategoryUid,
    string IncomeCategoryName,
    Guid? BookingUid,
    DateOnly IncomeDate,
    string Description,
    decimal Amount,
    string Currency,
    string? PaymentMethod,
    string? ReferenceNumber,
    string? Notes,
    DateTimeOffset CreationDate);
