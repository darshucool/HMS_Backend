using HMS.Modules.Finance.Domain.Common;
using HMS.Modules.Finance.Domain.Enums;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class ExpenseCategory : AuditableEntity
{
    private ExpenseCategory() { }

    public long OrganizationId { get; private set; }
    public long? ParentId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ExpenseGroup ExpenseGroup { get; private set; }
}
