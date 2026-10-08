using HMS.Modules.Finance.Domain.Common;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class IncomeCategory : AuditableEntity
{
    private IncomeCategory() { }

    public long OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
}
