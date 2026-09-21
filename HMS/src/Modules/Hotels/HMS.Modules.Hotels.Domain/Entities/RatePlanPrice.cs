using HMS.Modules.Hotels.Domain.Common;

namespace HMS.Modules.Hotels.Domain.Entities;

public sealed class RatePlanPrice : AuditableEntity
{
    private RatePlanPrice() { }

    public long OrganizationId { get; private set; }
    public long PropertyId { get; private set; }
    public Guid PropertyUid { get; private set; }
    public long RatePlanId { get; private set; }
    public Guid RatePlanUid { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public short? DayOfWeek { get; private set; }
    public decimal? AdultRate { get; private set; }
    public decimal? ChildRate { get; private set; }
    public decimal UnitRate { get; private set; }
    public int MinimumStay { get; private set; } = 1;

    public static RatePlanPrice Create(
        long organizationId,
        long propertyId,
        Guid propertyUid,
        long ratePlanId,
        Guid ratePlanUid,
        DateOnly startDate,
        DateOnly endDate,
        short? dayOfWeek,
        decimal? adultRate,
        decimal? childRate,
        decimal unitRate,
        int minimumStay,
        string actorSubject)
    {
        Validate(startDate, endDate, dayOfWeek, adultRate, childRate, unitRate, minimumStay);

        var price = new RatePlanPrice
        {
            Uid = Guid.NewGuid(),
            OrganizationId = organizationId,
            PropertyId = propertyId,
            PropertyUid = propertyUid,
            RatePlanId = ratePlanId,
            RatePlanUid = ratePlanUid,
            StartDate = startDate,
            EndDate = endDate,
            DayOfWeek = dayOfWeek,
            AdultRate = adultRate,
            ChildRate = childRate,
            UnitRate = unitRate,
            MinimumStay = minimumStay
        };

        price.MarkCreated(actorSubject);
        return price;
    }

    private static void Validate(
        DateOnly startDate,
        DateOnly endDate,
        short? dayOfWeek,
        decimal? adultRate,
        decimal? childRate,
        decimal unitRate,
        int minimumStay)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        if (dayOfWeek is < 0 or > 6)
            throw new ArgumentException("Day of week must be between 0 and 6.", nameof(dayOfWeek));
        if (unitRate < 0)
            throw new ArgumentException("Unit rate cannot be negative.", nameof(unitRate));
        if (adultRate < 0)
            throw new ArgumentException("Adult rate cannot be negative.", nameof(adultRate));
        if (childRate < 0)
            throw new ArgumentException("Child rate cannot be negative.", nameof(childRate));
        if (minimumStay <= 0)
            throw new ArgumentException("Minimum stay must be greater than zero.", nameof(minimumStay));
    }
}
