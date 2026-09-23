using Dapper;
using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Application.DTOs;
using HMS.Modules.Guests.Domain.Entities;
using HMS.Modules.Guests.Domain.Enums;

namespace HMS.Modules.Guests.Infrastructure.Persistence.Repositories;

public sealed class GuestRepository(IGuestsDbConnectionFactory connectionFactory)
    : IGuestRepository
{
    public async Task InsertAsync(Guest guest, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.guests
            (
                uid, organization_id, guest_type, title, first_name, last_name,
                display_name, phone, alternate_phone, email, nationality_code,
                date_of_birth, preferred_language, address, city, country_code,
                notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @GuestType, @Title, @FirstName, @LastName,
                @DisplayName, @Phone, @AlternatePhone, @Email, @NationalityCode,
                @DateOfBirth, @PreferredLanguage, @Address, @City, @CountryCode,
                @Notes, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            ToWriteParameters(guest),
            cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(Guest guest, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.guests
            SET guest_type = @GuestType,
                title = @Title,
                first_name = @FirstName,
                last_name = @LastName,
                display_name = @DisplayName,
                phone = @Phone,
                alternate_phone = @AlternatePhone,
                email = @Email,
                nationality_code = @NationalityCode,
                date_of_birth = @DateOfBirth,
                preferred_language = @PreferredLanguage,
                address = @Address,
                city = @City,
                country_code = @CountryCode,
                notes = @Notes,
                is_active = @IsActive,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE uid = @Uid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            ToWriteParameters(guest),
            cancellationToken: cancellationToken));
    }

    public async Task<Guest?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.id                    AS "Id",
                g.uid                   AS "Uid",
                g.organization_id       AS "OrganizationId",
                o.uid                   AS "OrganizationUid",
                g.guest_type            AS "GuestType",
                g.title                 AS "Title",
                g.first_name            AS "FirstName",
                g.last_name             AS "LastName",
                g.display_name          AS "DisplayName",
                g.phone                 AS "Phone",
                g.alternate_phone       AS "AlternatePhone",
                g.email                 AS "Email",
                rtrim(g.nationality_code) AS "NationalityCode",
                g.date_of_birth         AS "DateOfBirth",
                g.preferred_language    AS "PreferredLanguage",
                g.address               AS "Address",
                g.city                  AS "City",
                rtrim(g.country_code)   AS "CountryCode",
                g.notes                 AS "Notes",
                g.is_active             AS "IsActive",
                g.is_archived           AS "IsArchived",
                g.creation_date         AS "CreationDate",
                g.created_by            AS "CreatedBy",
                g.modified_date         AS "ModifiedDate",
                g.modified_by           AS "ModifiedBy"
            FROM hotel.guests g
            JOIN hotel.organizations o ON o.id = g.organization_id
            WHERE g.uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<GuestRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<IReadOnlyList<GuestDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        string? search,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.uid                   AS "Uid",
                o.uid                   AS "OrganizationUid",
                g.guest_type            AS "GuestType",
                g.title                 AS "Title",
                g.first_name            AS "FirstName",
                g.last_name             AS "LastName",
                g.display_name          AS "DisplayName",
                g.phone                 AS "Phone",
                g.alternate_phone       AS "AlternatePhone",
                g.email                 AS "Email",
                rtrim(g.nationality_code) AS "NationalityCode",
                g.date_of_birth         AS "DateOfBirth",
                g.preferred_language    AS "PreferredLanguage",
                g.address               AS "Address",
                g.city                  AS "City",
                rtrim(g.country_code)   AS "CountryCode",
                g.notes                 AS "Notes",
                g.is_active             AS "IsActive",
                g.creation_date         AS "CreationDate"
            FROM hotel.guests g
            JOIN hotel.organizations o ON o.id = g.organization_id
            JOIN hotel.properties p ON p.organization_id = g.organization_id
            WHERE p.uid = @PropertyUid
              AND g.is_archived = false
              AND
              (
                  @Search IS NULL
                  OR lower(g.display_name) LIKE @Search
                  OR lower(COALESCE(g.phone, '')) LIKE @Search
                  OR lower(COALESCE(g.email, '')) LIKE @Search
                  OR lower(COALESCE(g.first_name, '')) LIKE @Search
                  OR lower(COALESCE(g.last_name, '')) LIKE @Search
              )
            ORDER BY g.display_name, g.creation_date;
            """;

        var searchPattern = string.IsNullOrWhiteSpace(search)
            ? null
            : $"%{search.Trim().ToLowerInvariant()}%";

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<GuestListRow>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid, Search = searchPattern },
            cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<GuestDto>> SearchAsync(
        string actorSubject,
        bool isPlatformAdmin,
        string? phone,
        string? name,
        string? email,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.uid                   AS "Uid",
                o.uid                   AS "OrganizationUid",
                g.guest_type            AS "GuestType",
                g.title                 AS "Title",
                g.first_name            AS "FirstName",
                g.last_name             AS "LastName",
                g.display_name          AS "DisplayName",
                g.phone                 AS "Phone",
                g.alternate_phone       AS "AlternatePhone",
                g.email                 AS "Email",
                rtrim(g.nationality_code) AS "NationalityCode",
                g.date_of_birth         AS "DateOfBirth",
                g.preferred_language    AS "PreferredLanguage",
                g.address               AS "Address",
                g.city                  AS "City",
                rtrim(g.country_code)   AS "CountryCode",
                g.notes                 AS "Notes",
                g.is_active             AS "IsActive",
                g.creation_date         AS "CreationDate"
            FROM hotel.guests g
            JOIN hotel.organizations o ON o.id = g.organization_id
            WHERE g.is_archived = false
              AND
              (
                  @IsPlatformAdmin = true
                  OR EXISTS
                  (
                      SELECT 1
                      FROM hotel.app_users u
                      JOIN hotel.user_property_access upa ON upa.user_id = u.id
                      WHERE u.auth_subject = @ActorSubject
                        AND upa.organization_id = g.organization_id
                        AND u.is_active = true
                        AND u.is_archived = false
                        AND upa.is_active = true
                        AND upa.is_archived = false
                  )
              )
              AND (@Phone IS NULL OR lower(COALESCE(g.phone, '')) LIKE @Phone
                   OR lower(COALESCE(g.alternate_phone, '')) LIKE @Phone)
              AND (@Name IS NULL
                   OR lower(g.display_name) LIKE @Name
                   OR lower(COALESCE(g.first_name, '')) LIKE @Name
                   OR lower(COALESCE(g.last_name, '')) LIKE @Name)
              AND (@Email IS NULL OR lower(COALESCE(g.email, '')) LIKE @Email)
            ORDER BY g.display_name, g.creation_date;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<GuestListRow>(new CommandDefinition(
            sql,
            new
            {
                ActorSubject = actorSubject,
                IsPlatformAdmin = isPlatformAdmin,
                Phone = Like(phone),
                Name = Like(name),
                Email = Like(email)
            },
            cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<GuestBookingHistoryDto>> GetBookingHistoryAsync(
        Guid guestUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                b.uid                   AS "BookingUid",
                b.booking_number        AS "BookingNumber",
                p.uid                   AS "PropertyUid",
                p.name                  AS "PropertyName",
                b.status                AS "Status",
                b.check_in_date         AS "CheckInDate",
                b.check_out_date        AS "CheckOutDate",
                b.adults                AS "Adults",
                b.children              AS "Children",
                b.quoted_total          AS "QuotedTotal",
                b.currency              AS "Currency",
                COALESCE(bg.is_lead_guest, b.lead_guest_id = g.id) AS "IsLeadGuest",
                b.creation_date         AS "CreationDate"
            FROM hotel.guests g
            JOIN hotel.bookings b
              ON b.organization_id = g.organization_id
             AND b.is_archived = false
            JOIN hotel.properties p ON p.id = b.property_id
            LEFT JOIN hotel.booking_guests bg
              ON bg.booking_id = b.id
             AND bg.guest_id = g.id
             AND bg.is_archived = false
            WHERE g.uid = @GuestUid
              AND (b.lead_guest_id = g.id OR bg.id IS NOT NULL)
            ORDER BY b.check_in_date DESC, b.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<GuestBookingHistoryRow>(new CommandDefinition(
            sql,
            new { GuestUid = guestUid },
            cancellationToken: cancellationToken));

        return rows.Select(ToDto).ToList();
    }

    private static object ToWriteParameters(Guest guest) => new
    {
        guest.Uid,
        guest.OrganizationId,
        GuestType = guest.GuestType.ToDatabaseValue(),
        guest.Title,
        guest.FirstName,
        guest.LastName,
        guest.DisplayName,
        guest.Phone,
        guest.AlternatePhone,
        guest.Email,
        guest.NationalityCode,
        guest.DateOfBirth,
        guest.PreferredLanguage,
        guest.Address,
        guest.City,
        guest.CountryCode,
        guest.Notes,
        guest.IsActive,
        guest.IsArchived,
        guest.CreationDate,
        guest.CreatedBy,
        guest.ModifiedDate,
        guest.ModifiedBy
    };

    private static Guest ToDomain(GuestRow row) => Guest.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.OrganizationUid,
        GuestTypeMapper.FromDatabaseValue(row.GuestType),
        row.Title,
        row.FirstName,
        row.LastName,
        row.DisplayName,
        row.Phone,
        row.AlternatePhone,
        row.Email,
        row.NationalityCode,
        row.DateOfBirth,
        row.PreferredLanguage,
        row.Address,
        row.City,
        row.CountryCode,
        row.Notes,
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private static GuestDto ToDto(GuestListRow row) => new(
        row.Uid,
        row.OrganizationUid,
        row.GuestType,
        row.Title,
        row.FirstName,
        row.LastName,
        row.DisplayName,
        row.Phone,
        row.AlternatePhone,
        row.Email,
        row.NationalityCode,
        row.DateOfBirth,
        row.PreferredLanguage,
        row.Address,
        row.City,
        row.CountryCode,
        row.Notes,
        row.IsActive,
        ToDateTimeOffset(row.CreationDate));

    private static GuestBookingHistoryDto ToDto(GuestBookingHistoryRow row) => new(
        row.BookingUid,
        row.BookingNumber,
        row.PropertyUid,
        row.PropertyName,
        row.Status,
        row.CheckInDate,
        row.CheckOutDate,
        row.Adults,
        row.Children,
        row.QuotedTotal,
        row.Currency,
        row.IsLeadGuest,
        ToDateTimeOffset(row.CreationDate));

    private static string? Like(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : $"%{value.Trim().ToLowerInvariant()}%";

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class GuestRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public Guid OrganizationUid { get; init; }
        public string GuestType { get; init; } = string.Empty;
        public string? Title { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public string? AlternatePhone { get; init; }
        public string? Email { get; init; }
        public string? NationalityCode { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? PreferredLanguage { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? CountryCode { get; init; }
        public string? Notes { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }

    private sealed class GuestListRow
    {
        public Guid Uid { get; init; }
        public Guid OrganizationUid { get; init; }
        public string GuestType { get; init; } = string.Empty;
        public string? Title { get; init; }
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public string? AlternatePhone { get; init; }
        public string? Email { get; init; }
        public string? NationalityCode { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public string? PreferredLanguage { get; init; }
        public string? Address { get; init; }
        public string? City { get; init; }
        public string? CountryCode { get; init; }
        public string? Notes { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreationDate { get; init; }
    }

    private sealed class GuestBookingHistoryRow
    {
        public Guid BookingUid { get; init; }
        public string BookingNumber { get; init; } = string.Empty;
        public Guid PropertyUid { get; init; }
        public string PropertyName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateOnly CheckInDate { get; init; }
        public DateOnly CheckOutDate { get; init; }
        public int Adults { get; init; }
        public int Children { get; init; }
        public decimal? QuotedTotal { get; init; }
        public string Currency { get; init; } = string.Empty;
        public bool IsLeadGuest { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
