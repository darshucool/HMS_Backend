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
            new
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
                guest.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<GuestDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        string? search,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                g.uid                   AS Uid,
                o.uid                   AS OrganizationUid,
                g.guest_type            AS GuestType,
                g.title                 AS Title,
                g.first_name            AS FirstName,
                g.last_name             AS LastName,
                g.display_name          AS DisplayName,
                g.phone                 AS Phone,
                g.alternate_phone       AS AlternatePhone,
                g.email                 AS Email,
                rtrim(g.nationality_code) AS NationalityCode,
                g.date_of_birth         AS DateOfBirth,
                g.preferred_language    AS PreferredLanguage,
                g.address               AS Address,
                g.city                  AS City,
                rtrim(g.country_code)   AS CountryCode,
                g.notes                 AS Notes,
                g.is_active             AS IsActive,
                g.creation_date         AS CreationDate
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
        var items = await connection.QueryAsync<GuestDto>(new CommandDefinition(
            sql,
            new { PropertyUid = propertyUid, Search = searchPattern },
            cancellationToken: cancellationToken));

        return items.AsList();
    }
}
