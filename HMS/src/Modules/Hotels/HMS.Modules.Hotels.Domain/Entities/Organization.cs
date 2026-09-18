using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class Organization : AuditableEntity
{
    private Organization() { }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? LegalName { get; private set; }
    public string DefaultCurrency { get; private set; } = "LKR";
    public string Timezone { get; private set; } = "Asia/Colombo";
    public OrganizationStatus Status { get; private set; } = OrganizationStatus.Active;

    public static Organization Create(
        string code,
        string name,
        string? legalName,
        string currency,
        string timezone,
        string actorSubject)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(timezone);

        var organization = new Organization
        {
            Uid = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            LegalName = string.IsNullOrWhiteSpace(legalName) ? null : legalName.Trim(),
            DefaultCurrency = currency.Trim().ToUpperInvariant(),
            Timezone = timezone.Trim(),
            Status = OrganizationStatus.Active
        };

        organization.MarkCreated(actorSubject);
        return organization;
    }

    public static Organization Rehydrate(
        long id,
        Guid uid,
        string code,
        string name,
        string? legalName,
        string defaultCurrency,
        string timezone,
        OrganizationStatus status,
        bool isActive,
        bool isArchived,
        DateTimeOffset creationDate,
        string? createdBy,
        DateTimeOffset? modifiedDate,
        string? modifiedBy) =>
        new()
        {
            Id = id,
            Uid = uid,
            Code = code,
            Name = name,
            LegalName = legalName,
            DefaultCurrency = defaultCurrency,
            Timezone = timezone,
            Status = status,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };
}

