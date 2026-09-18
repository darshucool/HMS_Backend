using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class PropertyRepository(IHotelsDbConnectionFactory connectionFactory)
    : IPropertyRepository
{
    public async Task<Property?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.id                    AS Id,
                p.uid                   AS Uid,
                p.organization_id       AS OrganizationId,
                o.uid                   AS OrganizationUid,
                p.code                  AS Code,
                p.name                  AS Name,
                p.slug                  AS Slug,
                p.property_type         AS PropertyType,
                p.description           AS Description,
                p.address_line1         AS AddressLine1,
                p.address_line2         AS AddressLine2,
                p.city                  AS City,
                p.district              AS District,
                p.province              AS Province,
                p.postal_code           AS PostalCode,
                p.country_code          AS CountryCode,
                p.latitude              AS Latitude,
                p.longitude             AS Longitude,
                p.phone                 AS Phone,
                p.email                 AS Email,
                p.timezone              AS Timezone,
                p.default_currency      AS DefaultCurrency,
                p.status                AS Status,
                p.is_active             AS IsActive,
                p.is_archived           AS IsArchived,
                p.creation_date         AS CreationDate,
                p.created_by            AS CreatedBy,
                p.modified_date         AS ModifiedDate,
                p.modified_by           AS ModifiedBy
            FROM hotel.properties p
            JOIN hotel.organizations o ON o.id = p.organization_id
            WHERE p.uid = @PropertyUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<PropertyRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<PropertySettings?> GetSettingsAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                ps.id                       AS Id,
                ps.uid                      AS Uid,
                ps.organization_id          AS OrganizationId,
                ps.property_id              AS PropertyId,
                ps.check_in_time             AS CheckInTime,
                ps.check_out_time            AS CheckOutTime,
                ps.booking_number_prefix     AS BookingNumberPrefix,
                ps.invoice_number_prefix     AS InvoiceNumberPrefix,
                ps.tax_rate                  AS TaxRate,
                ps.service_charge_rate       AS ServiceChargeRate,
                ps.allow_overbooking         AS AllowOverbooking,
                ps.extra_settings::text      AS ExtraSettingsJson,
                ps.is_active                 AS IsActive,
                ps.is_archived               AS IsArchived,
                ps.creation_date             AS CreationDate,
                ps.created_by                AS CreatedBy,
                ps.modified_date             AS ModifiedDate,
                ps.modified_by               AS ModifiedBy
            FROM hotel.property_settings ps
            JOIN hotel.properties p ON p.id = ps.property_id
            WHERE p.uid = @PropertyUid
              AND ps.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<PropertySettingsRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return row is null ? null : PropertySettings.Rehydrate(
            row.Id,
            row.Uid,
            row.OrganizationId,
            row.PropertyId,
            row.CheckInTime,
            row.CheckOutTime,
            row.BookingNumberPrefix,
            row.InvoiceNumberPrefix,
            row.TaxRate,
            row.ServiceChargeRate,
            row.AllowOverbooking,
            row.ExtraSettingsJson,
            row.IsActive,
            row.IsArchived,
            row.CreationDate,
            row.CreatedBy,
            row.ModifiedDate,
            row.ModifiedBy);
    }

    public async Task<bool> CodeExistsAsync(
        long organizationId,
        string code,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.properties
                WHERE organization_id = @OrganizationId
                  AND code = @Code
                  AND is_archived = false
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { OrganizationId = organizationId, Code = code.Trim().ToUpperInvariant() },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.properties
                WHERE slug = @Slug
                  AND is_archived = false
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { Slug = slug.Trim().ToLowerInvariant() },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.app_users u
                JOIN hotel.user_property_access upa ON upa.user_id = u.id
                JOIN hotel.properties p
                  ON p.id = upa.property_id
                 AND p.organization_id = upa.organization_id
                WHERE u.auth_subject = @ActorSubject
                  AND p.uid = @PropertyUid
                  AND u.is_active = true
                  AND u.is_archived = false
                  AND upa.is_active = true
                  AND upa.is_archived = false
                  AND p.is_archived = false
                  AND
                  (
                      @RequireManager = false
                      OR upa.role_code IN ('PROPERTY_ADMIN', 'MANAGER', 'PLATFORM_ADMIN')
                  )
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new
            {
                ActorSubject = actorSubject,
                PropertyUid = propertyUid,
                RequireManager = requireManager
            },
            cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(
        Property property,
        PropertySettings defaultSettings,
        CancellationToken cancellationToken)
    {
        const string propertySql = """
            INSERT INTO hotel.properties
            (
                uid, organization_id, code, name, slug, property_type, description,
                address_line1, address_line2, city, district, province, postal_code,
                country_code, latitude, longitude, phone, email, timezone,
                default_currency, status, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @Code, @Name, @Slug, @PropertyType, @Description,
                @AddressLine1, @AddressLine2, @City, @District, @Province, @PostalCode,
                @CountryCode, @Latitude, @Longitude, @Phone, @Email, @Timezone,
                @DefaultCurrency, @Status, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            )
            RETURNING id;
            """;

        const string settingsSql = """
            INSERT INTO hotel.property_settings
            (
                uid, organization_id, property_id, check_in_time, check_out_time,
                booking_number_prefix, invoice_number_prefix, tax_rate,
                service_charge_rate, allow_overbooking, extra_settings,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @CheckInTime, @CheckOutTime,
                @BookingNumberPrefix, @InvoiceNumberPrefix, @TaxRate,
                @ServiceChargeRate, @AllowOverbooking, CAST(@ExtraSettingsJson AS jsonb),
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var propertyId = await connection.ExecuteScalarAsync<long>(new CommandDefinition(
                propertySql,
                new
                {
                    property.Uid,
                    property.OrganizationId,
                    property.Code,
                    property.Name,
                    property.Slug,
                    PropertyType = property.PropertyType.ToString().ToUpperInvariant(),
                    property.Description,
                    property.AddressLine1,
                    property.AddressLine2,
                    property.City,
                    property.District,
                    property.Province,
                    property.PostalCode,
                    property.CountryCode,
                    property.Latitude,
                    property.Longitude,
                    property.Phone,
                    property.Email,
                    property.Timezone,
                    property.DefaultCurrency,
                    Status = property.Status.ToString().ToUpperInvariant(),
                    property.IsActive,
                    property.IsArchived,
                    property.CreationDate,
                    property.CreatedBy
                },
                transaction,
                cancellationToken: cancellationToken));

            await connection.ExecuteAsync(new CommandDefinition(
                settingsSql,
                new
                {
                    defaultSettings.Uid,
                    defaultSettings.OrganizationId,
                    PropertyId = propertyId,
                    defaultSettings.CheckInTime,
                    defaultSettings.CheckOutTime,
                    defaultSettings.BookingNumberPrefix,
                    defaultSettings.InvoiceNumberPrefix,
                    defaultSettings.TaxRate,
                    defaultSettings.ServiceChargeRate,
                    defaultSettings.AllowOverbooking,
                    defaultSettings.ExtraSettingsJson,
                    defaultSettings.IsActive,
                    defaultSettings.IsArchived,
                    defaultSettings.CreationDate,
                    defaultSettings.CreatedBy
                },
                transaction,
                cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(Property property, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.properties
            SET name = @Name,
                property_type = @PropertyType,
                description = @Description,
                address_line1 = @AddressLine1,
                address_line2 = @AddressLine2,
                city = @City,
                district = @District,
                province = @Province,
                postal_code = @PostalCode,
                country_code = @CountryCode,
                latitude = @Latitude,
                longitude = @Longitude,
                phone = @Phone,
                email = @Email,
                timezone = @Timezone,
                default_currency = @DefaultCurrency,
                status = @Status,
                is_active = @IsActive,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE uid = @Uid
              AND is_archived = false;
            """;

        var parameters = new
        {
            property.Uid,
            property.Name,
            PropertyType = property.PropertyType.ToString().ToUpperInvariant(),
            property.Description,
            property.AddressLine1,
            property.AddressLine2,
            property.City,
            property.District,
            property.Province,
            property.PostalCode,
            property.CountryCode,
            property.Latitude,
            property.Longitude,
            property.Phone,
            property.Email,
            property.Timezone,
            property.DefaultCurrency,
            Status = property.Status.ToString().ToUpperInvariant(),
            property.IsActive,
            property.ModifiedDate,
            property.ModifiedBy
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task UpdateSettingsAsync(
        PropertySettings settings,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.property_settings
            SET check_in_time = @CheckInTime,
                check_out_time = @CheckOutTime,
                booking_number_prefix = @BookingNumberPrefix,
                invoice_number_prefix = @InvoiceNumberPrefix,
                tax_rate = @TaxRate,
                service_charge_rate = @ServiceChargeRate,
                allow_overbooking = @AllowOverbooking,
                extra_settings = CAST(@ExtraSettingsJson AS jsonb),
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @Id
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                settings.Id,
                settings.CheckInTime,
                settings.CheckOutTime,
                settings.BookingNumberPrefix,
                settings.InvoiceNumberPrefix,
                settings.TaxRate,
                settings.ServiceChargeRate,
                settings.AllowOverbooking,
                settings.ExtraSettingsJson,
                settings.ModifiedDate,
                settings.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<MyPropertyDto>> GetMyPropertiesAsync(
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.uid                       AS Uid,
                p.code                      AS Code,
                p.name                      AS Name,
                p.slug                      AS Slug,
                p.property_type             AS PropertyType,
                upa.role_code               AS RoleCode,
                upa.is_default_property     AS IsDefaultProperty,
                p.status                    AS Status
            FROM hotel.app_users u
            JOIN hotel.user_property_access upa ON upa.user_id = u.id
            JOIN hotel.properties p
              ON p.id = upa.property_id
             AND p.organization_id = upa.organization_id
            WHERE u.auth_subject = @ActorSubject
              AND u.is_active = true
              AND u.is_archived = false
              AND upa.is_active = true
              AND upa.is_archived = false
              AND p.is_active = true
              AND p.is_archived = false
            ORDER BY upa.is_default_property DESC, p.name;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var items = await connection.QueryAsync<MyPropertyDto>(new CommandDefinition(
            sql,
            new { ActorSubject = actorSubject },
            cancellationToken: cancellationToken));

        return items.AsList();
    }

    private static Property ToDomain(PropertyRow row) => Property.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.OrganizationUid,
        row.Code,
        row.Name,
        row.Slug,
        Enum.Parse<PropertyType>(row.PropertyType, true),
        row.Description,
        row.AddressLine1,
        row.AddressLine2,
        row.City,
        row.District,
        row.Province,
        row.PostalCode,
        row.CountryCode,
        row.Latitude,
        row.Longitude,
        row.Phone,
        row.Email,
        row.Timezone,
        row.DefaultCurrency,
        Enum.Parse<PropertyStatus>(row.Status, true),
        row.IsActive,
        row.IsArchived,
        row.CreationDate,
        row.CreatedBy,
        row.ModifiedDate,
        row.ModifiedBy);

    private sealed record PropertyRow(
        long Id,
        Guid Uid,
        long OrganizationId,
        Guid OrganizationUid,
        string Code,
        string Name,
        string Slug,
        string PropertyType,
        string? Description,
        string? AddressLine1,
        string? AddressLine2,
        string? City,
        string? District,
        string? Province,
        string? PostalCode,
        string CountryCode,
        decimal? Latitude,
        decimal? Longitude,
        string? Phone,
        string? Email,
        string Timezone,
        string DefaultCurrency,
        string Status,
        bool IsActive,
        bool IsArchived,
        DateTimeOffset CreationDate,
        string? CreatedBy,
        DateTimeOffset? ModifiedDate,
        string? ModifiedBy);

    private sealed record PropertySettingsRow(
        long Id,
        Guid Uid,
        long OrganizationId,
        long PropertyId,
        TimeOnly CheckInTime,
        TimeOnly CheckOutTime,
        string BookingNumberPrefix,
        string InvoiceNumberPrefix,
        decimal TaxRate,
        decimal ServiceChargeRate,
        bool AllowOverbooking,
        string ExtraSettingsJson,
        bool IsActive,
        bool IsArchived,
        DateTimeOffset CreationDate,
        string? CreatedBy,
        DateTimeOffset? ModifiedDate,
        string? ModifiedBy);
}

