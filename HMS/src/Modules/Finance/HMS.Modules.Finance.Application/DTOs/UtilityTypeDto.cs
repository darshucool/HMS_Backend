namespace HMS.Modules.Finance.Application.DTOs;

public sealed record UtilityTypeDto(
    Guid Uid,
    string Code,
    string Name,
    string? UnitOfMeasure,
    bool IsMetered,
    bool IsActive);
