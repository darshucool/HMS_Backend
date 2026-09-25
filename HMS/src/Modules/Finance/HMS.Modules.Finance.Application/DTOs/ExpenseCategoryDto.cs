namespace HMS.Modules.Finance.Application.DTOs;

public sealed record ExpenseCategoryDto(
    Guid Uid,
    Guid? ParentUid,
    string Code,
    string Name,
    string ExpenseGroup,
    bool IsActive);
