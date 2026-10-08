namespace HMS.Modules.Finance.Application.DTOs;

public sealed record IncomeCategoryDto(
    Guid Uid,
    string Code,
    string Name,
    bool IsActive);
