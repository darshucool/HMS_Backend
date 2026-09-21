using HMS.Modules.Hotels.Domain.Common;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class MealPlan : AuditableEntity
{
    private MealPlan() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IncludesBreakfast { get; private set; }
    public bool IncludesLunch { get; private set; }
    public bool IncludesDinner { get; private set; }
    public bool AllowByo { get; private set; }

    public static MealPlan Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string code,
        string name,
        string? description,
        bool includesBreakfast,
        bool includesLunch,
        bool includesDinner,
        bool allowByo,
        string actorSubject)
    {
        var mealPlan = new MealPlan
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            Code = Required(code, nameof(code), 30).ToUpperInvariant(),
            Name = Required(name, nameof(name), 100),
            Description = Clean(description),
            IncludesBreakfast = includesBreakfast,
            IncludesLunch = includesLunch,
            IncludesDinner = includesDinner,
            AllowByo = allowByo
        };

        mealPlan.MarkCreated(actorSubject);
        return mealPlan;
    }

    public static MealPlan Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        string code,
        string name,
        string? description,
        bool includesBreakfast,
        bool includesLunch,
        bool includesDinner,
        bool allowByo,
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
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            Code = code,
            Name = name,
            Description = description,
            IncludesBreakfast = includesBreakfast,
            IncludesLunch = includesLunch,
            IncludesDinner = includesDinner,
            AllowByo = allowByo,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static string Required(string value, string name, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{name} cannot exceed {maxLength} characters.", name);

        return trimmed;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
