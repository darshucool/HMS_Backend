using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class OtherIncomeRepository(IFinanceDbConnectionFactory connectionFactory)
    : IOtherIncomeRepository
{
    public async Task<IncomeCategoryRef?> GetCategoryAsync(
        Guid categoryUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", uid AS "Uid", name AS "Name", organization_id AS "OrganizationId"
            FROM hotel.income_categories
            WHERE uid = @CategoryUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<IncomeCategoryRef>(
            new CommandDefinition(sql, new { CategoryUid = categoryUid }, cancellationToken: cancellationToken));
    }

    public async Task<BookingRef?> GetBookingAsync(Guid bookingUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                uid             AS "Uid",
                organization_id AS "OrganizationId",
                property_id     AS "PropertyId"
            FROM hotel.bookings
            WHERE uid = @BookingUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<BookingRef>(
            new CommandDefinition(sql, new { BookingUid = bookingUid }, cancellationToken: cancellationToken));
    }

    public async Task InsertAsync(OtherIncomeEntity income, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.other_income
            (
                uid, organization_id, property_id, income_category_id, booking_id,
                income_date, description, amount, currency, payment_method,
                reference_number, notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @IncomeCategoryId, @BookingId,
                @IncomeDate, @Description, @Amount, @Currency, @PaymentMethod,
                @ReferenceNumber, @Notes, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                income.Uid,
                income.OrganizationId,
                income.PropertyId,
                income.IncomeCategoryId,
                income.BookingId,
                income.IncomeDate,
                income.Description,
                income.Amount,
                income.Currency,
                PaymentMethod = income.PaymentMethod?.ToDatabaseValue(),
                income.ReferenceNumber,
                income.Notes,
                income.IsActive,
                income.IsArchived,
                income.CreationDate,
                income.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<OtherIncomeDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                i.uid               AS "Uid",
                p.uid               AS "PropertyUid",
                c.uid               AS "IncomeCategoryUid",
                c.name              AS "IncomeCategoryName",
                b.uid               AS "BookingUid",
                i.income_date       AS "IncomeDate",
                i.description       AS "Description",
                i.amount            AS "Amount",
                rtrim(i.currency)   AS "Currency",
                i.payment_method    AS "PaymentMethod",
                i.reference_number  AS "ReferenceNumber",
                i.notes             AS "Notes",
                i.creation_date     AS "CreationDate"
            FROM hotel.other_income i
            JOIN hotel.properties p ON p.id = i.property_id
            JOIN hotel.income_categories c ON c.id = i.income_category_id
            LEFT JOIN hotel.bookings b ON b.id = i.booking_id
            WHERE p.uid = @PropertyUid
              AND i.is_archived = false
            ORDER BY i.income_date DESC, i.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<OtherIncomeListRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return rows.Select(row => new OtherIncomeDto(
            row.Uid,
            row.PropertyUid,
            row.IncomeCategoryUid,
            row.IncomeCategoryName,
            row.BookingUid,
            row.IncomeDate,
            row.Description,
            row.Amount,
            row.Currency,
            row.PaymentMethod,
            row.ReferenceNumber,
            row.Notes,
            ToDateTimeOffset(row.CreationDate))).ToList();
    }

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class OtherIncomeListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid IncomeCategoryUid { get; init; }
        public string IncomeCategoryName { get; init; } = string.Empty;
        public Guid? BookingUid { get; init; }
        public DateOnly IncomeDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
