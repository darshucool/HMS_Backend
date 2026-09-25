using Dapper;
using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Application.DTOs;
using HMS.Modules.Booking.Domain.Enums;
using HotelBooking = HMS.Modules.Booking.Domain.Entities.Booking;
using Npgsql;

namespace HMS.Modules.Booking.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository(IBookingDbConnectionFactory connectionFactory)
    : IBookingRepository
{
    public async Task<string> NextBookingNumberAsync(
        long propertyId,
        string prefix,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM hotel.bookings
            WHERE property_id = @PropertyId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var count = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, new { PropertyId = propertyId }, cancellationToken: cancellationToken));

        var cleanedPrefix = string.IsNullOrWhiteSpace(prefix) ? "BKG" : prefix.Trim().ToUpperInvariant();
        return $"{cleanedPrefix}{(count + 1):D5}";
    }

    public async Task<GuestBookingContext?> GetGuestAsync(
        Guid guestUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", organization_id AS "OrganizationId"
            FROM hotel.guests
            WHERE uid = @GuestUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<GuestBookingContext>(
            new CommandDefinition(sql, new { GuestUid = guestUid }, cancellationToken: cancellationToken));
    }

    public async Task<AccommodationTypeBookingContext?> GetAccommodationTypeAsync(
        Guid accommodationTypeUid,
        long propertyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id"
            FROM hotel.accommodation_types
            WHERE uid = @AccommodationTypeUid
              AND property_id = @PropertyId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AccommodationTypeBookingContext>(
            new CommandDefinition(
                sql,
                new { AccommodationTypeUid = accommodationTypeUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));
    }

    public async Task<long?> GetUnitIdAsync(
        Guid unitUid,
        long propertyId,
        long accommodationTypeId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM hotel.accommodation_units
            WHERE uid = @UnitUid
              AND property_id = @PropertyId
              AND accommodation_type_id = @AccommodationTypeId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<long?>(
            new CommandDefinition(
                sql,
                new
                {
                    UnitUid = unitUid,
                    PropertyId = propertyId,
                    AccommodationTypeId = accommodationTypeId
                },
                cancellationToken: cancellationToken));
    }

    public async Task<long?> GetRatePlanIdAsync(
        Guid ratePlanUid,
        long propertyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM hotel.rate_plans
            WHERE uid = @RatePlanUid
              AND property_id = @PropertyId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<long?>(
            new CommandDefinition(
                sql,
                new { RatePlanUid = ratePlanUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));
    }

    public async Task<long?> GetMealPlanIdAsync(
        Guid mealPlanUid,
        long propertyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM hotel.meal_plans
            WHERE uid = @MealPlanUid
              AND property_id = @PropertyId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<long?>(
            new CommandDefinition(
                sql,
                new { MealPlanUid = mealPlanUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(
        HotelBooking booking,
        IReadOnlyList<BookingUnitLine> units,
        CancellationToken cancellationToken)
    {
        const string insertBookingSql = """
            INSERT INTO hotel.bookings
            (
                uid, organization_id, property_id, booking_number, lead_guest_id,
                booking_source, external_reference, status, check_in_date, check_out_date,
                adults, children, infants, currency, discount_amount, tax_amount,
                service_charge, quoted_total, arrival_time, departure_time,
                special_requests, internal_notes, is_active, is_archived,
                creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @BookingNumber, @LeadGuestId,
                @BookingSource, @ExternalReference, 'PENDING', @CheckInDate, @CheckOutDate,
                @Adults, @Children, @Infants, @Currency, @DiscountAmount, @TaxAmount,
                @ServiceCharge, @QuotedTotal, @ArrivalTime, @DepartureTime,
                @SpecialRequests, @InternalNotes, @IsActive, @IsArchived,
                @CreationDate, @CreatedBy
            )
            RETURNING id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var bookingId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                insertBookingSql,
                new
                {
                    booking.Uid,
                    booking.OrganizationId,
                    booking.PropertyId,
                    booking.BookingNumber,
                    booking.LeadGuestId,
                    BookingSource = booking.BookingSource.ToDatabaseValue(),
                    booking.ExternalReference,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.Adults,
                    booking.Children,
                    booking.Infants,
                    booking.Currency,
                    booking.DiscountAmount,
                    booking.TaxAmount,
                    booking.ServiceCharge,
                    booking.QuotedTotal,
                    booking.ArrivalTime,
                    booking.DepartureTime,
                    booking.SpecialRequests,
                    booking.InternalNotes,
                    booking.IsActive,
                    booking.IsArchived,
                    booking.CreationDate,
                    booking.CreatedBy
                },
                transaction,
                cancellationToken: cancellationToken));

            if (booking.LeadGuestId is long leadGuestId)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO hotel.booking_guests
                    (
                        uid, organization_id, property_id, booking_id, guest_id,
                        is_lead_guest, is_active, is_archived, creation_date, created_by
                    )
                    VALUES
                    (
                        @Uid, @OrganizationId, @PropertyId, @BookingId, @GuestId,
                        true, true, false, @CreationDate, @CreatedBy
                    );
                    """,
                    new
                    {
                        Uid = Guid.NewGuid(),
                        booking.OrganizationId,
                        booking.PropertyId,
                        BookingId = bookingId,
                        GuestId = leadGuestId,
                        booking.CreationDate,
                        booking.CreatedBy
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            foreach (var unit in units)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO hotel.booking_units
                    (
                        uid, organization_id, property_id, booking_id, accommodation_type_id,
                        unit_id, rate_plan_id, meal_plan_id, check_in_date, check_out_date,
                        adults, children, unit_quantity, guest_count, pricing_basis,
                        unit_rate, discount_amount, tax_amount, total_amount, allocation_status,
                        notes, is_active, is_archived, creation_date, created_by
                    )
                    VALUES
                    (
                        @Uid, @OrganizationId, @PropertyId, @BookingId, @AccommodationTypeId,
                        @UnitId, @RatePlanId, @MealPlanId, @CheckInDate, @CheckOutDate,
                        @Adults, @Children, @UnitQuantity, @GuestCount, @PricingBasis,
                        @UnitRate, @DiscountAmount, @TaxAmount, @TotalAmount, 'HELD',
                        @Notes, true, false, @CreationDate, @CreatedBy
                    );
                    """,
                    new
                    {
                        Uid = Guid.NewGuid(),
                        booking.OrganizationId,
                        booking.PropertyId,
                        BookingId = bookingId,
                        unit.AccommodationTypeId,
                        unit.UnitId,
                        unit.RatePlanId,
                        unit.MealPlanId,
                        unit.CheckInDate,
                        unit.CheckOutDate,
                        unit.Adults,
                        unit.Children,
                        unit.UnitQuantity,
                        unit.GuestCount,
                        unit.PricingBasis,
                        unit.UnitRate,
                        unit.DiscountAmount,
                        unit.TaxAmount,
                        unit.TotalAmount,
                        unit.Notes,
                        booking.CreationDate,
                        booking.CreatedBy
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState is "23505")
        {
            throw new InvalidOperationException("A booking with this number already exists for the property.");
        }
        catch (PostgresException exception) when (exception.SqlState is "23P01")
        {
            throw new InvalidOperationException("The selected unit is already booked for these dates.");
        }
    }

    public async Task<IReadOnlyList<BookingDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.uid                   AS "Uid",
                p.uid                   AS "PropertyUid",
                b.booking_number        AS "BookingNumber",
                g.uid                   AS "LeadGuestUid",
                g.display_name          AS "LeadGuestName",
                b.booking_source        AS "BookingSource",
                b.status                AS "Status",
                b.check_in_date         AS "CheckInDate",
                b.check_out_date        AS "CheckOutDate",
                b.number_of_nights      AS "Nights",
                b.adults                AS "Adults",
                b.children              AS "Children",
                b.infants               AS "Infants",
                rtrim(b.currency)       AS "Currency",
                b.quoted_total          AS "QuotedTotal",
                b.special_requests      AS "SpecialRequests",
                b.creation_date         AS "CreationDate"
            FROM hotel.bookings b
            JOIN hotel.properties p ON p.id = b.property_id
            LEFT JOIN hotel.guests g ON g.id = b.lead_guest_id
            WHERE p.uid = @PropertyUid
              AND b.is_archived = false
            ORDER BY b.check_in_date DESC, b.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<BookingListRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public async Task<BookingCalendarDto> GetBookingCalendarAsync(
        long propertyId,
        Guid propertyUid,
        DateOnly from,
        DateOnly to,
        long? accommodationTypeId,
        CancellationToken cancellationToken)
    {
        const string unitsSql = """
            SELECT
                u.id            AS "UnitId",
                u.uid           AS "UnitUid",
                u.unit_code     AS "UnitCode",
                u.unit_name     AS "UnitName",
                at.uid          AS "AccommodationTypeUid",
                at.name         AS "AccommodationTypeName"
            FROM hotel.accommodation_units u
            JOIN hotel.accommodation_types at ON at.id = u.accommodation_type_id
            WHERE u.property_id = @PropertyId
              AND u.is_archived = false
              AND (@AccommodationTypeId IS NULL OR u.accommodation_type_id = @AccommodationTypeId)
            ORDER BY at.sort_order, at.name, u.unit_code;
            """;

        const string bookingsSql = """
            SELECT
                u.uid                       AS "UnitUid",
                b.uid                       AS "BookingUid",
                b.booking_number            AS "BookingNumber",
                COALESCE(g.display_name, b.booking_number) AS "Label",
                b.status                    AS "Status",
                bu.check_in_date            AS "StartDate",
                bu.check_out_date           AS "EndDate"
            FROM hotel.booking_units bu
            JOIN hotel.bookings b ON b.id = bu.booking_id
            JOIN hotel.accommodation_units u ON u.id = bu.unit_id
            LEFT JOIN hotel.guests g ON g.id = b.lead_guest_id
            WHERE bu.property_id = @PropertyId
              AND bu.unit_id IS NOT NULL
              AND bu.is_archived = false
              AND b.is_archived = false
              AND bu.allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN')
              AND bu.check_in_date < @To
              AND bu.check_out_date > @From
              AND (@AccommodationTypeId IS NULL OR bu.accommodation_type_id = @AccommodationTypeId);
            """;

        const string blocksSql = """
            SELECT
                u.uid           AS "UnitUid",
                COALESCE(ub.reason, ub.block_type) AS "Label",
                ub.block_type   AS "Status",
                ub.start_date   AS "StartDate",
                ub.end_date     AS "EndDate"
            FROM hotel.unit_blocks ub
            JOIN hotel.accommodation_units u ON u.id = ub.unit_id
            WHERE ub.property_id = @PropertyId
              AND ub.is_archived = false
              AND ub.is_active = true
              AND ub.start_date < @To
              AND ub.end_date > @From
              AND (@AccommodationTypeId IS NULL OR u.accommodation_type_id = @AccommodationTypeId);
            """;

        var parameters = new
        {
            PropertyId = propertyId,
            From = from,
            To = to,
            AccommodationTypeId = accommodationTypeId
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var units = (await connection.QueryAsync<CalendarUnitRow>(
            new CommandDefinition(unitsSql, parameters, cancellationToken: cancellationToken))).AsList();
        var bookings = (await connection.QueryAsync<CalendarBookingRow>(
            new CommandDefinition(bookingsSql, parameters, cancellationToken: cancellationToken))).AsList();
        var blocks = (await connection.QueryAsync<CalendarBlockRow>(
            new CommandDefinition(blocksSql, parameters, cancellationToken: cancellationToken))).AsList();

        var segmentsByUnit = units.ToDictionary(unit => unit.UnitUid, _ => new List<BookingCalendarSegmentDto>());

        foreach (var booking in bookings)
        {
            if (!segmentsByUnit.TryGetValue(booking.UnitUid, out var segments))
                continue;

            segments.Add(new BookingCalendarSegmentDto(
                "BOOKING",
                booking.BookingUid,
                booking.BookingNumber,
                booking.Label,
                booking.Status,
                booking.StartDate,
                booking.EndDate));
        }

        foreach (var block in blocks)
        {
            if (!segmentsByUnit.TryGetValue(block.UnitUid, out var segments))
                continue;

            segments.Add(new BookingCalendarSegmentDto(
                "BLOCK",
                null,
                null,
                block.Label,
                block.Status,
                block.StartDate,
                block.EndDate));
        }

        return new BookingCalendarDto(
            propertyUid,
            from,
            to,
            units.Select(unit => new BookingCalendarUnitDto(
                unit.UnitUid,
                unit.UnitCode,
                unit.UnitName,
                unit.AccommodationTypeUid,
                unit.AccommodationTypeName,
                segmentsByUnit[unit.UnitUid]
                    .OrderBy(segment => segment.StartDate)
                    .ThenBy(segment => segment.SegmentType)
                    .ToList())).ToList());
    }

    public async Task<HotelBooking?> GetByUidAsync(Guid bookingUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.id                    AS "Id",
                b.uid                   AS "Uid",
                b.organization_id       AS "OrganizationId",
                b.property_id           AS "PropertyId",
                p.uid                   AS "PropertyUid",
                b.booking_number        AS "BookingNumber",
                b.lead_guest_id         AS "LeadGuestId",
                g.uid                   AS "LeadGuestUid",
                b.booking_source        AS "BookingSource",
                b.status                AS "Status",
                b.external_reference    AS "ExternalReference",
                b.check_in_date         AS "CheckInDate",
                b.check_out_date        AS "CheckOutDate",
                b.adults                AS "Adults",
                b.children              AS "Children",
                b.infants               AS "Infants",
                rtrim(b.currency)       AS "Currency",
                b.discount_amount       AS "DiscountAmount",
                b.tax_amount            AS "TaxAmount",
                b.service_charge        AS "ServiceCharge",
                b.quoted_total          AS "QuotedTotal",
                b.arrival_time          AS "ArrivalTime",
                b.departure_time        AS "DepartureTime",
                b.special_requests      AS "SpecialRequests",
                b.internal_notes        AS "InternalNotes",
                b.cancellation_reason   AS "CancellationReason",
                b.confirmed_at          AS "ConfirmedAt",
                b.checked_in_at         AS "CheckedInAt",
                b.checked_out_at        AS "CheckedOutAt",
                b.cancelled_at          AS "CancelledAt",
                b.is_active             AS "IsActive",
                b.is_archived           AS "IsArchived",
                b.creation_date         AS "CreationDate",
                b.created_by            AS "CreatedBy",
                b.modified_date         AS "ModifiedDate",
                b.modified_by           AS "ModifiedBy"
            FROM hotel.bookings b
            JOIN hotel.properties p ON p.id = b.property_id
            LEFT JOIN hotel.guests g ON g.id = b.lead_guest_id
            WHERE b.uid = @BookingUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<BookingHeaderRow>(
            new CommandDefinition(sql, new { BookingUid = bookingUid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<BookingDetailDto?> GetDetailByUidAsync(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var booking = await GetByUidAsync(bookingUid, cancellationToken);
        if (booking is null || booking.IsArchived)
            return null;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var guestName = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(
            """
            SELECT display_name
            FROM hotel.guests
            WHERE id = @GuestId;
            """,
            new { GuestId = booking.LeadGuestId },
            cancellationToken: cancellationToken));

        var units = (await connection.QueryAsync<BookingUnitRow>(new CommandDefinition(
            """
            SELECT
                bu.uid                  AS "Uid",
                at.uid                  AS "AccommodationTypeUid",
                u.uid                   AS "UnitUid",
                bu.check_in_date        AS "CheckInDate",
                bu.check_out_date       AS "CheckOutDate",
                bu.adults               AS "Adults",
                bu.children             AS "Children",
                bu.unit_quantity        AS "UnitQuantity",
                bu.guest_count          AS "GuestCount",
                bu.pricing_basis        AS "PricingBasis",
                bu.unit_rate            AS "UnitRate",
                bu.total_amount         AS "TotalAmount",
                bu.allocation_status    AS "AllocationStatus"
            FROM hotel.booking_units bu
            JOIN hotel.accommodation_types at ON at.id = bu.accommodation_type_id
            LEFT JOIN hotel.accommodation_units u ON u.id = bu.unit_id
            WHERE bu.booking_id = @BookingId
              AND bu.is_archived = false
            ORDER BY bu.creation_date;
            """,
            new { BookingId = booking.Id },
            cancellationToken: cancellationToken))).AsList();

        var guests = (await connection.QueryAsync<BookingGuestRow>(new CommandDefinition(
            """
            SELECT
                g.uid           AS "GuestUid",
                g.display_name  AS "DisplayName",
                bg.is_lead_guest AS "IsLeadGuest"
            FROM hotel.booking_guests bg
            JOIN hotel.guests g ON g.id = bg.guest_id
            WHERE bg.booking_id = @BookingId
              AND bg.is_archived = false
            ORDER BY bg.is_lead_guest DESC, g.display_name;
            """,
            new { BookingId = booking.Id },
            cancellationToken: cancellationToken))).AsList();

        var nights = booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber;
        return new BookingDetailDto(
            booking.Uid,
            booking.PropertyUid,
            booking.BookingNumber,
            booking.LeadGuestUid,
            guestName,
            booking.BookingSource.ToDatabaseValue(),
            booking.ExternalReference,
            booking.Status.ToDatabaseValue(),
            booking.CheckInDate,
            booking.CheckOutDate,
            nights,
            booking.Adults,
            booking.Children,
            booking.Infants,
            booking.Currency,
            booking.DiscountAmount,
            booking.TaxAmount,
            booking.ServiceCharge,
            booking.QuotedTotal,
            booking.ArrivalTime,
            booking.DepartureTime,
            booking.SpecialRequests,
            booking.InternalNotes,
            booking.CancellationReason,
            booking.ConfirmedAt,
            booking.CheckedInAt,
            booking.CheckedOutAt,
            booking.CancelledAt,
            booking.CreationDate,
            units.Select(unit => new BookingUnitDto(
                unit.Uid,
                unit.AccommodationTypeUid,
                unit.UnitUid,
                unit.CheckInDate,
                unit.CheckOutDate,
                unit.Adults,
                unit.Children,
                unit.UnitQuantity,
                unit.GuestCount,
                unit.PricingBasis,
                unit.UnitRate,
                unit.TotalAmount,
                unit.AllocationStatus)).ToList(),
            guests.Select(guest => new BookingGuestDto(
                guest.GuestUid,
                guest.DisplayName,
                guest.IsLeadGuest)).ToList());
    }

    public async Task<BookingUnitContext?> GetBookingUnitAsync(
        long bookingId,
        Guid bookingUnitUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id                      AS "Id",
                accommodation_type_id   AS "AccommodationTypeId",
                unit_id                 AS "UnitId",
                allocation_status       AS "AllocationStatus"
            FROM hotel.booking_units
            WHERE uid = @BookingUnitUid
              AND booking_id = @BookingId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<BookingUnitContext>(
            new CommandDefinition(
                sql,
                new { BookingUnitUid = bookingUnitUid, BookingId = bookingId },
                cancellationToken: cancellationToken));
    }

    public async Task AssignUnitAsync(
        long bookingUnitId,
        long unitId,
        string allocationStatus,
        DateTimeOffset modifiedDate,
        string? modifiedBy,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_units
            SET unit_id = @UnitId,
                allocation_status = @AllocationStatus,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @BookingUnitId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

        try
        {
            var updated = await connection.ExecuteAsync(new CommandDefinition(
                sql,
                new
                {
                    UnitId = unitId,
                    AllocationStatus = allocationStatus,
                    ModifiedDate = modifiedDate,
                    ModifiedBy = modifiedBy,
                    BookingUnitId = bookingUnitId
                },
                cancellationToken: cancellationToken));

            if (updated == 0)
                throw new InvalidOperationException("Booking unit line was not found.");
        }
        catch (PostgresException exception) when (exception.SqlState is "23P01")
        {
            throw new InvalidOperationException("The selected unit is already booked for these dates.");
        }
    }

    public async Task<BookingGuestDto> AddGuestAsync(
        HotelBooking booking,
        Guid guestUid,
        long guestId,
        long? bookingUnitId,
        bool isLeadGuest,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var existing = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.booking_guests
                WHERE booking_id = @BookingId
                  AND guest_id = @GuestId
                  AND is_archived = false
            );
            """,
            new { BookingId = booking.Id, GuestId = guestId },
            transaction,
            cancellationToken: cancellationToken));

        if (existing)
            throw new InvalidOperationException("This guest is already on the booking.");

        var displayName = await connection.ExecuteScalarAsync<string>(new CommandDefinition(
            """
            SELECT display_name
            FROM hotel.guests
            WHERE id = @GuestId;
            """,
            new { GuestId = guestId },
            transaction,
            cancellationToken: cancellationToken));

        var now = DateTimeOffset.UtcNow;
        if (isLeadGuest)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                UPDATE hotel.booking_guests
                SET is_lead_guest = false,
                    modified_date = @ModifiedDate,
                    modified_by = @ModifiedBy
                WHERE booking_id = @BookingId
                  AND is_lead_guest = true;

                UPDATE hotel.bookings
                SET lead_guest_id = @GuestId,
                    modified_date = @ModifiedDate,
                    modified_by = @ModifiedBy
                WHERE id = @BookingId;
                """,
                new
                {
                    BookingId = booking.Id,
                    GuestId = guestId,
                    ModifiedDate = now,
                    ModifiedBy = actorSubject
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await connection.ExecuteAsync(new CommandDefinition(
            """
            INSERT INTO hotel.booking_guests
            (
                uid, organization_id, property_id, booking_id, booking_unit_id, guest_id,
                is_lead_guest, checked_in_at, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @BookingId, @BookingUnitId, @GuestId,
                @IsLeadGuest, @CheckedInAt, true, false, @CreationDate, @CreatedBy
            )
            ON CONFLICT (booking_id, guest_id) DO UPDATE
            SET is_archived = false,
                is_active = true,
                is_lead_guest = EXCLUDED.is_lead_guest,
                booking_unit_id = EXCLUDED.booking_unit_id,
                checked_in_at = COALESCE(hotel.booking_guests.checked_in_at, EXCLUDED.checked_in_at),
                modified_date = EXCLUDED.creation_date,
                modified_by = EXCLUDED.created_by
            WHERE hotel.booking_guests.is_archived = true;
            """,
            new
            {
                Uid = Guid.NewGuid(),
                booking.OrganizationId,
                booking.PropertyId,
                BookingId = booking.Id,
                BookingUnitId = bookingUnitId,
                GuestId = guestId,
                IsLeadGuest = isLeadGuest,
                CheckedInAt = booking.Status == BookingStatus.CheckedIn ? now : (DateTimeOffset?)null,
                CreationDate = now,
                CreatedBy = actorSubject
            },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
        return new BookingGuestDto(guestUid, displayName ?? string.Empty, isLeadGuest);
    }

    public async Task<BookingGuestContext?> GetBookingGuestAsync(
        long bookingId,
        long guestId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                organization_id AS "OrganizationId",
                booking_id      AS "BookingId",
                guest_id        AS "GuestId",
                booking_unit_id AS "BookingUnitId",
                is_lead_guest   AS "IsLeadGuest"
            FROM hotel.booking_guests
            WHERE booking_id = @BookingId
              AND guest_id = @GuestId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<BookingGuestContext>(
            new CommandDefinition(
                sql,
                new { BookingId = bookingId, GuestId = guestId },
                cancellationToken: cancellationToken));
    }

    public async Task RemoveGuestAsync(
        long bookingGuestId,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.booking_guests
            SET is_active = false,
                is_archived = true,
                is_lead_guest = false,
                checked_out_at = CASE
                    WHEN checked_in_at IS NOT NULL AND checked_out_at IS NULL
                    THEN CURRENT_TIMESTAMP
                    ELSE checked_out_at
                END,
                modified_date = CURRENT_TIMESTAMP,
                modified_by = @ActorSubject
            WHERE id = @BookingGuestId
              AND is_archived = false
              AND is_lead_guest = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { BookingGuestId = bookingGuestId, ActorSubject = actorSubject },
            cancellationToken: cancellationToken));

        if (updated == 0)
            throw new InvalidOperationException("The lead guest cannot be removed. Assign a new lead guest first.");
    }

    public async Task<IReadOnlyList<BookingStatusHistoryDto>> GetStatusHistoryAsync(
        long bookingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid         AS "Uid",
                old_status  AS "OldStatus",
                new_status  AS "NewStatus",
                reason      AS "Reason",
                changed_at  AS "ChangedAt",
                changed_by  AS "ChangedBy"
            FROM hotel.booking_status_history
            WHERE booking_id = @BookingId
              AND is_archived = false
            ORDER BY changed_at, id;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<BookingStatusHistoryRow>(
            new CommandDefinition(sql, new { BookingId = bookingId }, cancellationToken: cancellationToken));

        return rows.Select(row => new BookingStatusHistoryDto(
            row.Uid,
            row.OldStatus,
            row.NewStatus,
            row.Reason,
            ToDateTimeOffset(row.ChangedAt),
            row.ChangedBy)).ToList();
    }

    public async Task UpdateAsync(HotelBooking booking, CancellationToken cancellationToken)
    {
        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                UPDATE hotel.bookings
                SET lead_guest_id = @LeadGuestId,
                    booking_source = @BookingSource,
                    external_reference = @ExternalReference,
                    status = @Status,
                    check_in_date = @CheckInDate,
                    check_out_date = @CheckOutDate,
                    adults = @Adults,
                    children = @Children,
                    infants = @Infants,
                    currency = @Currency,
                    discount_amount = @DiscountAmount,
                    tax_amount = @TaxAmount,
                    service_charge = @ServiceCharge,
                    quoted_total = @QuotedTotal,
                    arrival_time = @ArrivalTime,
                    departure_time = @DepartureTime,
                    special_requests = @SpecialRequests,
                    internal_notes = @InternalNotes,
                    cancellation_reason = @CancellationReason,
                    confirmed_at = @ConfirmedAt,
                    checked_in_at = @CheckedInAt,
                    checked_out_at = @CheckedOutAt,
                    cancelled_at = @CancelledAt,
                    modified_date = @ModifiedDate,
                    modified_by = @ModifiedBy
                WHERE uid = @Uid
                  AND is_archived = false;
                """,
                new
                {
                    booking.Uid,
                    booking.LeadGuestId,
                    BookingSource = booking.BookingSource.ToDatabaseValue(),
                    booking.ExternalReference,
                    Status = booking.Status.ToDatabaseValue(),
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.Adults,
                    booking.Children,
                    booking.Infants,
                    booking.Currency,
                    booking.DiscountAmount,
                    booking.TaxAmount,
                    booking.ServiceCharge,
                    booking.QuotedTotal,
                    booking.ArrivalTime,
                    booking.DepartureTime,
                    booking.SpecialRequests,
                    booking.InternalNotes,
                    booking.CancellationReason,
                    booking.ConfirmedAt,
                    booking.CheckedInAt,
                    booking.CheckedOutAt,
                    booking.CancelledAt,
                    booking.ModifiedDate,
                    booking.ModifiedBy
                },
                transaction,
                cancellationToken: cancellationToken));

            var allocationStatus = booking.Status switch
            {
                BookingStatus.Confirmed => "CONFIRMED",
                BookingStatus.Cancelled => "CANCELLED",
                BookingStatus.CheckedIn => "CHECKED_IN",
                BookingStatus.CheckedOut => "RELEASED",
                _ => null
            };

            if (allocationStatus is not null)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    UPDATE hotel.booking_units
                    SET allocation_status = @AllocationStatus,
                        modified_date = @ModifiedDate,
                        modified_by = @ModifiedBy
                    WHERE booking_id = @BookingId
                      AND is_archived = false
                      AND allocation_status IN ('HELD', 'CONFIRMED', 'CHECKED_IN');
                    """,
                    new
                    {
                        AllocationStatus = allocationStatus,
                        booking.ModifiedDate,
                        booking.ModifiedBy,
                        BookingId = booking.Id
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            if (booking.Status == BookingStatus.CheckedIn)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    UPDATE hotel.booking_guests
                    SET checked_in_at = COALESCE(checked_in_at, @CheckedInAt),
                        modified_date = @ModifiedDate,
                        modified_by = @ModifiedBy
                    WHERE booking_id = @BookingId
                      AND is_archived = false
                      AND is_active = true;
                    """,
                    new
                    {
                        booking.CheckedInAt,
                        booking.ModifiedDate,
                        booking.ModifiedBy,
                        BookingId = booking.Id
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            if (booking.Status == BookingStatus.CheckedOut)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    UPDATE hotel.booking_guests
                    SET checked_out_at = COALESCE(checked_out_at, @CheckedOutAt),
                        modified_date = @ModifiedDate,
                        modified_by = @ModifiedBy
                    WHERE booking_id = @BookingId
                      AND is_archived = false
                      AND is_active = true;
                    """,
                    new
                    {
                        booking.CheckedOutAt,
                        booking.ModifiedDate,
                        booking.ModifiedBy,
                        BookingId = booking.Id
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await connection.ExecuteAsync(new CommandDefinition(
                """
                UPDATE hotel.booking_units
                SET check_in_date = @CheckInDate,
                    check_out_date = @CheckOutDate,
                    modified_date = @ModifiedDate,
                    modified_by = @ModifiedBy
                WHERE booking_id = @BookingId
                  AND is_archived = false;
                """,
                new
                {
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.ModifiedDate,
                    booking.ModifiedBy,
                    BookingId = booking.Id
                },
                transaction,
                cancellationToken: cancellationToken));

            if (booking.LeadGuestId is long leadGuestId)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    UPDATE hotel.booking_guests
                    SET is_lead_guest = false,
                        modified_date = @ModifiedDate,
                        modified_by = @ModifiedBy
                    WHERE booking_id = @BookingId
                      AND is_lead_guest = true
                      AND guest_id <> @GuestId;

                    INSERT INTO hotel.booking_guests
                    (
                        uid, organization_id, property_id, booking_id, guest_id,
                        is_lead_guest, is_active, is_archived, creation_date, created_by
                    )
                    VALUES
                    (
                        @Uid, @OrganizationId, @PropertyId, @BookingId, @GuestId,
                        true, true, false, @ModifiedDate, @ModifiedBy
                    )
                    ON CONFLICT (booking_id, guest_id) DO UPDATE
                    SET is_lead_guest = true,
                        is_active = true,
                        is_archived = false,
                        modified_date = EXCLUDED.creation_date,
                        modified_by = EXCLUDED.created_by;
                    """,
                    new
                    {
                        Uid = Guid.NewGuid(),
                        booking.OrganizationId,
                        booking.PropertyId,
                        BookingId = booking.Id,
                        GuestId = leadGuestId,
                        booking.ModifiedDate,
                        booking.ModifiedBy
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            if (booking.PreviousStatus is not null &&
                !string.Equals(booking.PreviousStatus, booking.Status.ToDatabaseValue(), StringComparison.Ordinal))
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    """
                    INSERT INTO hotel.booking_status_history
                    (
                        uid, organization_id, property_id, booking_id,
                        old_status, new_status, reason, changed_at, changed_by,
                        is_active, is_archived, creation_date, created_by
                    )
                    VALUES
                    (
                        @Uid, @OrganizationId, @PropertyId, @BookingId,
                        @OldStatus, @NewStatus, @Reason, @ChangedAt, @ChangedBy,
                        true, false, @ChangedAt, @ChangedBy
                    );
                    """,
                    new
                    {
                        Uid = Guid.NewGuid(),
                        booking.OrganizationId,
                        booking.PropertyId,
                        BookingId = booking.Id,
                        OldStatus = booking.PreviousStatus,
                        NewStatus = booking.Status.ToDatabaseValue(),
                        Reason = booking.CancellationReason,
                        ChangedAt = booking.ModifiedDate,
                        ChangedBy = booking.ModifiedBy
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState is "23P01")
        {
            throw new InvalidOperationException("The selected unit is already booked for these dates.");
        }
    }

    private static BookingDto ToDto(BookingListRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.BookingNumber,
        row.LeadGuestUid,
        row.LeadGuestName,
        row.BookingSource,
        row.Status,
        row.CheckInDate,
        row.CheckOutDate,
        row.Nights,
        row.Adults,
        row.Children,
        row.Infants,
        row.Currency,
        row.QuotedTotal,
        row.SpecialRequests,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class BookingListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public Guid? LeadGuestUid { get; init; }
        public string? LeadGuestName { get; init; }
        public string BookingSource { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Nights { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public int Infants { get; init; }
        public string Currency { get; init; } = string.Empty;
        public decimal? QuotedTotal { get; init; }
        public string? SpecialRequests { get; init; }
        public DateTime CreationDate { get; init; }
    }

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private static HotelBooking ToDomain(BookingHeaderRow row) => HotelBooking.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.BookingNumber,
        row.LeadGuestId,
        row.LeadGuestUid,
        BookingSourceMapper.FromDatabaseValue(row.BookingSource),
        BookingStatusMapper.FromDatabaseValue(row.Status),
        row.ExternalReference,
        row.CheckInDate,
        row.CheckOutDate,
        row.Adults,
        row.Children,
        row.Infants,
        row.Currency,
        row.DiscountAmount,
        row.TaxAmount,
        row.ServiceCharge,
        row.QuotedTotal,
        row.ArrivalTime,
        row.DepartureTime,
        row.SpecialRequests,
        row.InternalNotes,
        row.CancellationReason,
        ToDateTimeOffset(row.ConfirmedAt),
        ToDateTimeOffset(row.CheckedInAt),
        ToDateTimeOffset(row.CheckedOutAt),
        ToDateTimeOffset(row.CancelledAt),
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private sealed class BookingHeaderRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public long PropertyId { get; init; }
        public Guid PropertyUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public long? LeadGuestId { get; init; }
        public Guid? LeadGuestUid { get; init; }
        public string BookingSource { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? ExternalReference { get; init; }
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public int Infants { get; init; }
        public string Currency { get; init; } = string.Empty;
        public decimal DiscountAmount { get; init; }
        public decimal TaxAmount { get; init; }
        public decimal ServiceCharge { get; init; }
        public decimal? QuotedTotal { get; init; }
        public TimeOnly? ArrivalTime { get; init; }
        public TimeOnly? DepartureTime { get; init; }
        public string? SpecialRequests { get; init; }
        public string? InternalNotes { get; init; }
        public string? CancellationReason { get; init; }
        public DateTime? ConfirmedAt { get; init; }
        public DateTime? CheckedInAt { get; init; }
        public DateTime? CheckedOutAt { get; init; }
        public DateTime? CancelledAt { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }

    private sealed class BookingUnitRow
    {
        public Guid Uid { get; init; }
        public Guid AccommodationTypeUid { get; init; }
        public Guid? UnitUid { get; init; }
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public int UnitQuantity { get; init; }
        public int GuestCount { get; init; }
        public string PricingBasis { get; init; } = string.Empty;
        public decimal UnitRate { get; init; }
        public decimal TotalAmount { get; init; }
        public string AllocationStatus { get; init; } = string.Empty;
    }

    private sealed class BookingGuestRow
    {
        public Guid GuestUid { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public bool IsLeadGuest { get; init; }
    }

    private sealed class BookingStatusHistoryRow
    {
        public Guid Uid { get; init; }
        public string? OldStatus { get; init; }
        public string NewStatus { get; init; } = string.Empty;
        public string? Reason { get; init; }
        public DateTime ChangedAt { get; init; }
        public string? ChangedBy { get; init; }
    }

    private sealed class CalendarUnitRow
    {
        public long UnitId { get; init; }
        public Guid UnitUid { get; init; }
        public string UnitCode { get; init; } = string.Empty;
        public string? UnitName { get; init; }
        public Guid AccommodationTypeUid { get; init; }
        public string AccommodationTypeName { get; init; } = string.Empty;
    }

    private sealed class CalendarBookingRow
    {
        public Guid UnitUid { get; init; }
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public string? Label { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
    }

    private sealed class CalendarBlockRow
    {
        public Guid UnitUid { get; init; }
        public string? Label { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateOnly StartDate { get; init; }
        public DateOnly EndDate { get; init; }
    }
}
