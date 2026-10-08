namespace HMS.Modules.Finance.Api.Contracts;

public sealed record UpsertUtilityTypeRequest(
    string Code,
    string Name,
    string? UnitOfMeasure = null,
    bool IsMetered = true);
