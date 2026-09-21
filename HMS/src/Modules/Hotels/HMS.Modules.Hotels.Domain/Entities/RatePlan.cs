using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class RatePlan : AuditableEntity
{
    private RatePlan() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public long AccommodationTypeId { get; private set; }
    public Guid AccommodationTypeUid { get; private set; }
    public long? MealPlanId { get; private set; }
    public Guid? MealPlanUid { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public PricingBasis PricingBasis { get; private set; }
    public string Currency { get; private set; } = "LKR";
    public string? Description { get; private set; }
    public bool IsRefundable { get; private set; } = true;

    public static RatePlan Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long accommodationTypeId,
        Guid accommodationTypeUid,
        long? mealPlanId,
        Guid? mealPlanUid,
        string code,
        string name,
        PricingBasis pricingBasis,
        string currency,
        string? description,
        bool isRefundable,
        string actorSubject)
    {
        var ratePlan = new RatePlan
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            AccommodationTypeId = accommodationTypeId,
            AccommodationTypeUid = accommodationTypeUid,
            MealPlanId = mealPlanId,
            MealPlanUid = mealPlanUid,
            Code = Required(code, nameof(code), 30).ToUpperInvariant(),
            Name = Required(name, nameof(name), 150),
            PricingBasis = pricingBasis,
            Currency = RequiredCurrency(currency),
            Description = Clean(description),
            IsRefundable = isRefundable
        };

        ratePlan.MarkCreated(actorSubject);
        return ratePlan;
    }

    public static RatePlan Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long accommodationTypeId,
        Guid accommodationTypeUid,
        long? mealPlanId,
        Guid? mealPlanUid,
        string code,
        string name,
        PricingBasis pricingBasis,
        string currency,
        string? description,
        bool isRefundable,
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
            AccommodationTypeId = accommodationTypeId,
            AccommodationTypeUid = accommodationTypeUid,
            MealPlanId = mealPlanId,
            MealPlanUid = mealPlanUid,
            Code = code,
            Name = name,
            PricingBasis = pricingBasis,
            Currency = currency,
            Description = description,
            IsRefundable = isRefundable,
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

    private static string RequiredCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency, nameof(currency));
        var trimmed = currency.Trim().ToUpperInvariant();
        if (trimmed.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code.", nameof(currency));

        return trimmed;
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
