namespace HMS.Modules.Finance.Api.Contracts;

public sealed record UpsertIncomeCategoryRequest(
    string Code,
    string Name,
    bool IsActive = true);
