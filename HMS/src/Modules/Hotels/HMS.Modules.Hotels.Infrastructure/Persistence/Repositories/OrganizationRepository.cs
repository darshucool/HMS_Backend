using Dapper;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;

namespace HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(IHotelsDbConnectionFactory connectionFactory)
    : IOrganizationRepository
{
    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.organizations
                WHERE code = @Code
                  AND is_archived = false
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(sql, new { Code = code.ToUpperInvariant() }, cancellationToken: cancellationToken));
    }

    public async Task<Organization?> GetByUidAsync(Guid uid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id                  AS "Id",
                uid                 AS "Uid",
                code                AS "Code",
                name                AS "Name",
                legal_name          AS "LegalName",
                default_currency    AS "DefaultCurrency",
                timezone            AS "Timezone",
                status              AS "Status",
                is_active           AS "IsActive",
                is_archived         AS "IsArchived",
                creation_date       AS "CreationDate",
                created_by          AS "CreatedBy",
                modified_date       AS "ModifiedDate",
                modified_by         AS "ModifiedBy"
            FROM hotel.organizations
            WHERE uid = @Uid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<OrganizationRow>(
            new CommandDefinition(sql, new { Uid = uid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task InsertAsync(Organization organization, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.organizations
            (
                uid, code, name, legal_name, default_currency, timezone, status,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @Code, @Name, @LegalName, @DefaultCurrency, @Timezone, @Status,
                @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        var parameters = new
        {
            organization.Uid,
            organization.Code,
            organization.Name,
            organization.LegalName,
            organization.DefaultCurrency,
            organization.Timezone,
            Status = organization.Status.ToString().ToUpperInvariant(),
            organization.IsActive,
            organization.IsArchived,
            organization.CreationDate,
            organization.CreatedBy
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task<(IReadOnlyList<OrganizationDto> Items, long TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                uid                 AS "Uid",
                code                AS "Code",
                name                AS "Name",
                legal_name          AS "LegalName",
                default_currency    AS "DefaultCurrency",
                timezone            AS "Timezone",
                status              AS "Status",
                is_active           AS "IsActive",
                creation_date       AS "CreationDate"
            FROM hotel.organizations
            WHERE is_archived = false
              AND (@Search IS NULL
                   OR code ILIKE '%' || @Search || '%'
                   OR name ILIKE '%' || @Search || '%'
                   OR legal_name ILIKE '%' || @Search || '%')
            ORDER BY name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM hotel.organizations
            WHERE is_archived = false
              AND (@Search IS NULL
                   OR code ILIKE '%' || @Search || '%'
                   OR name ILIKE '%' || @Search || '%'
                   OR legal_name ILIKE '%' || @Search || '%');
            """;

        var parameters = new
        {
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        using var result = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var rows = (await result.ReadAsync<OrganizationListRow>()).AsList();
        var totalCount = await result.ReadSingleAsync<long>();
        return (rows.Select(ToDto).ToList(), totalCount);
    }

    private static Organization ToDomain(OrganizationRow row) => Organization.Rehydrate(
        row.Id,
        row.Uid,
        row.Code,
        row.Name,
        row.LegalName,
        row.DefaultCurrency,
        row.Timezone,
        Enum.Parse<OrganizationStatus>(row.Status, true),
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private static OrganizationDto ToDto(OrganizationListRow row) => new(
        row.Uid,
        row.Code,
        row.Name,
        row.LegalName,
        row.DefaultCurrency,
        row.Timezone,
        row.Status,
        row.IsActive,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class OrganizationRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? LegalName { get; init; }
        public string DefaultCurrency { get; init; } = string.Empty;
        public string Timezone { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }

    private sealed class OrganizationListRow
    {
        public Guid Uid { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? LegalName { get; init; }
        public string DefaultCurrency { get; init; } = string.Empty;
        public string Timezone { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreationDate { get; init; }
    }
}

