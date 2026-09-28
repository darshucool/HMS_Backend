using HMS.Modules.Finance.Domain.Common;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class UtilityType : AuditableEntity
{
    private UtilityType() { }

    public long OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? UnitOfMeasure { get; private set; }
    public bool IsMetered { get; private set; } = true;
}
