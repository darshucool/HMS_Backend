using HMS.Modules.Hotels.Domain.Common;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class UnitBlock : AuditableEntity
{
    private UnitBlock() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public long UnitId { get; private set; }
    public Guid UnitUid { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public UnitBlockType BlockType { get; private set; }
    public string? Reason { get; private set; }

    public static UnitBlock Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long unitId,
        Guid unitUid,
        DateOnly startDate,
        DateOnly endDate,
        UnitBlockType blockType,
        string? reason,
        string actorSubject)
    {
        ValidateDates(startDate, endDate);

        var block = new UnitBlock
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            UnitId = unitId,
            UnitUid = unitUid,
            StartDate = startDate,
            EndDate = endDate,
            BlockType = blockType,
            Reason = Clean(reason)
        };

        block.MarkCreated(actorSubject);
        return block;
    }

    public void Archive(string actorSubject)
    {
        IsArchived = true;
        IsActive = false;
        MarkModified(actorSubject);
    }

    public static UnitBlock Rehydrate(
        long id,
        Guid uid,
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long unitId,
        Guid unitUid,
        DateOnly startDate,
        DateOnly endDate,
        UnitBlockType blockType,
        string? reason,
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
            UnitId = unitId,
            UnitUid = unitUid,
            StartDate = startDate,
            EndDate = endDate,
            BlockType = blockType,
            Reason = reason,
            IsActive = isActive,
            IsArchived = isArchived,
            CreationDate = creationDate,
            CreatedBy = createdBy,
            ModifiedDate = modifiedDate,
            ModifiedBy = modifiedBy
        };

    private static void ValidateDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.", nameof(endDate));
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
