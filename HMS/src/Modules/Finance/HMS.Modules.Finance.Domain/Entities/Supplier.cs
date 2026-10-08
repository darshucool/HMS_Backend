using HMS.Modules.Finance.Domain.Common;

namespace HMS.Modules.Finance.Domain.Entities;

public sealed class Supplier : AuditableEntity
{
    private Supplier() { }

    public long OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? Notes { get; private set; }
}
