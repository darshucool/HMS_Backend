using Dapper;
using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.DTOs;

namespace HMS.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class BookingChargeRepository(IPaymentsDbConnectionFactory connectionFactory)
    : IBookingChargeRepository
{
    public async Task<ChargeTypeRef?> GetChargeTypeAsync(Guid chargeTypeUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                uid             AS "Uid",
                name            AS "Name",
                organization_id AS "OrganizationId",
                property_id     AS "PropertyId"
            FROM hotel.booking_charge_types
            WHERE uid = @ChargeTypeUid
              AND is_archived = false
              AND is_active = true;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ChargeTypeRef>(
            new CommandDefinition(sql, new { ChargeTypeUid = chargeTypeUid }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<BookingChargeDto>> GetByBookingIdAsync(
        long bookingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.uid               AS "Uid",
                b.uid               AS "BookingUid",
                bu.uid              AS "BookingUnitUid",
                ct.uid              AS "ChargeTypeUid",
                ct.name             AS "ChargeTypeName",
                c.service_date      AS "ServiceDate",
                c.description       AS "Description",
                c.quantity          AS "Quantity",
                c.unit_price        AS "UnitPrice",
                c.discount_amount   AS "DiscountAmount",
                c.tax_amount        AS "TaxAmount",
                c.total_amount      AS "TotalAmount",
                c.notes             AS "Notes",
                c.creation_date     AS "CreationDate"
            FROM hotel.booking_charges c
            JOIN hotel.bookings b ON b.id = c.booking_id
            JOIN hotel.booking_charge_types ct ON ct.id = c.charge_type_id
            LEFT JOIN hotel.booking_units bu ON bu.id = c.booking_unit_id
            WHERE c.booking_id = @BookingId
              AND c.is_archived = false
            ORDER BY c.service_date DESC, c.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ChargeRow>(
            new CommandDefinition(sql, new { BookingId = bookingId }, cancellationToken: cancellationToken));
        return rows.Select(ToDto).ToList();
    }

    public async Task<BookingChargeContext?> GetByUidAsync(Guid chargeUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.id                AS "Id",
                c.uid               AS "Uid",
                c.organization_id   AS "OrganizationId",
                c.property_id       AS "PropertyId",
                p.uid               AS "PropertyUid",
                c.booking_id        AS "BookingId",
                c.is_archived       AS "IsArchived"
            FROM hotel.booking_charges c
            JOIN hotel.properties p ON p.id = c.property_id
            WHERE c.uid = @ChargeUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<BookingChargeContext>(
            new CommandDefinition(sql, new { ChargeUid = chargeUid }, cancellationToken: cancellationToken));
    }

    public async Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        long bookingId,
        long? bookingUnitId,
        long chargeTypeId,
        DateOnly serviceDate,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.booking_charges
            (
                uid, organization_id, property_id, booking_id, booking_unit_id, charge_type_id,
                service_date, description, quantity, unit_price, discount_amount, tax_amount,
                notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @BookingId, @BookingUnitId, @ChargeTypeId,
                @ServiceDate, @Description, @Quantity, @UnitPrice, @DiscountAmount, @TaxAmount,
                @Notes, true, false, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Uid = uid,
                OrganizationId = organizationId,
                PropertyId = propertyId,
                BookingId = bookingId,
                BookingUnitId = bookingUnitId,
                ChargeTypeId = chargeTypeId,
                ServiceDate = serviceDate,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                Notes = notes,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task UpdateAsync(
        long chargeId,
        long chargeTypeId,
        long? bookingUnitId,
        DateOnly serviceDate,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_charges
            SET charge_type_id = @ChargeTypeId,
                booking_unit_id = @BookingUnitId,
                service_date = @ServiceDate,
                description = @Description,
                quantity = @Quantity,
                unit_price = @UnitPrice,
                discount_amount = @DiscountAmount,
                tax_amount = @TaxAmount,
                notes = @Notes,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @ChargeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                ChargeId = chargeId,
                ChargeTypeId = chargeTypeId,
                BookingUnitId = bookingUnitId,
                ServiceDate = serviceDate,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                Notes = notes,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(long chargeId, string actorSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_charges
            SET is_active = false,
                is_archived = true,
                modified_date = CURRENT_TIMESTAMP,
                modified_by = @ActorSubject
            WHERE id = @ChargeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { ChargeId = chargeId, ActorSubject = actorSubject },
            cancellationToken: cancellationToken));
    }

    public async Task<BookingChargeDto?> GetDetailByUidAsync(Guid chargeUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.uid               AS "Uid",
                b.uid               AS "BookingUid",
                bu.uid              AS "BookingUnitUid",
                ct.uid              AS "ChargeTypeUid",
                ct.name             AS "ChargeTypeName",
                c.service_date      AS "ServiceDate",
                c.description       AS "Description",
                c.quantity          AS "Quantity",
                c.unit_price        AS "UnitPrice",
                c.discount_amount   AS "DiscountAmount",
                c.tax_amount        AS "TaxAmount",
                c.total_amount      AS "TotalAmount",
                c.notes             AS "Notes",
                c.creation_date     AS "CreationDate"
            FROM hotel.booking_charges c
            JOIN hotel.bookings b ON b.id = c.booking_id
            JOIN hotel.booking_charge_types ct ON ct.id = c.charge_type_id
            LEFT JOIN hotel.booking_units bu ON bu.id = c.booking_unit_id
            WHERE c.uid = @ChargeUid
              AND c.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ChargeRow>(
            new CommandDefinition(sql, new { ChargeUid = chargeUid }, cancellationToken: cancellationToken));
        return row is null ? null : ToDto(row);
    }

    private static BookingChargeDto ToDto(ChargeRow row) => new(
        row.Uid,
        row.BookingUid,
        row.BookingUnitUid,
        row.ChargeTypeUid,
        row.ChargeTypeName,
        row.ServiceDate,
        row.Description,
        row.Quantity,
        row.UnitPrice,
        row.DiscountAmount,
        row.TaxAmount,
        row.TotalAmount,
        row.Notes,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class ChargeRow
    {
        public Guid Uid { get; init; }
        public Guid BookingUid { get; init; }
        public Guid? BookingUnitUid { get; init; }
        public Guid ChargeTypeUid { get; init; }
        public string ChargeTypeName { get; init; } = string.Empty;
        public DateOnly ServiceDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
