using Dapper;
using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using ExpenseEntity = HMS.Modules.Finance.Domain.Entities.Expense;

namespace HMS.Modules.Finance.Infrastructure.Persistence.Repositories;

public sealed class FinancePropertyAccessRepository(IFinanceDbConnectionFactory connectionFactory)
    : IFinancePropertyAccess
{
    public async Task<FinancePropertyContext?> GetByUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                organization_id AS "OrganizationId",
                is_archived     AS "IsArchived"
            FROM hotel.properties
            WHERE uid = @PropertyUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<FinancePropertyContext>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));
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
}

public sealed class ExpenseRepository(IFinanceDbConnectionFactory connectionFactory)
    : IExpenseRepository
{
    public async Task<ExpenseCategoryRef?> GetCategoryAsync(
        Guid categoryUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", uid AS "Uid", name AS "Name", organization_id AS "OrganizationId"
            FROM hotel.expense_categories
            WHERE uid = @CategoryUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ExpenseCategoryRef>(
            new CommandDefinition(sql, new { CategoryUid = categoryUid }, cancellationToken: cancellationToken));
    }

    public async Task<SupplierRef?> GetSupplierAsync(Guid supplierUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", uid AS "Uid", name AS "Name", organization_id AS "OrganizationId"
            FROM hotel.suppliers
            WHERE uid = @SupplierUid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<SupplierRef>(
            new CommandDefinition(sql, new { SupplierUid = supplierUid }, cancellationToken: cancellationToken));
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

    public async Task InsertAsync(ExpenseEntity expense, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO hotel.expenses
            (
                uid, organization_id, property_id, expense_category_id, supplier_id, booking_id,
                expense_date, description, amount, currency, payment_method, reference_number,
                receipt_url, notes, is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @ExpenseCategoryId, @SupplierId, @BookingId,
                @ExpenseDate, @Description, @Amount, @Currency, @PaymentMethod, @ReferenceNumber,
                @ReceiptUrl, @Notes, @IsActive, @IsArchived, @CreationDate, @CreatedBy
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                expense.Uid,
                expense.OrganizationId,
                expense.PropertyId,
                expense.ExpenseCategoryId,
                expense.SupplierId,
                expense.BookingId,
                expense.ExpenseDate,
                expense.Description,
                expense.Amount,
                expense.Currency,
                PaymentMethod = expense.PaymentMethod?.ToDatabaseValue(),
                expense.ReferenceNumber,
                expense.ReceiptUrl,
                expense.Notes,
                expense.IsActive,
                expense.IsArchived,
                expense.CreationDate,
                expense.CreatedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                e.uid                   AS "Uid",
                p.uid                   AS "PropertyUid",
                c.uid                   AS "ExpenseCategoryUid",
                c.name                  AS "ExpenseCategoryName",
                s.uid                   AS "SupplierUid",
                s.name                  AS "SupplierName",
                b.uid                   AS "BookingUid",
                e.expense_date          AS "ExpenseDate",
                e.description           AS "Description",
                e.amount                AS "Amount",
                rtrim(e.currency)       AS "Currency",
                e.payment_method        AS "PaymentMethod",
                e.reference_number      AS "ReferenceNumber",
                e.receipt_url           AS "ReceiptUrl",
                e.notes                 AS "Notes",
                e.creation_date         AS "CreationDate"
            FROM hotel.expenses e
            JOIN hotel.properties p ON p.id = e.property_id
            JOIN hotel.expense_categories c ON c.id = e.expense_category_id
            LEFT JOIN hotel.suppliers s ON s.id = e.supplier_id
            LEFT JOIN hotel.bookings b ON b.id = e.booking_id
            WHERE p.uid = @PropertyUid
              AND e.is_archived = false
            ORDER BY e.expense_date DESC, e.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ExpenseListRow>(
            new CommandDefinition(sql, new { PropertyUid = propertyUid }, cancellationToken: cancellationToken));

        return rows.Select(row => new ExpenseDto(
            row.Uid,
            row.PropertyUid,
            row.ExpenseCategoryUid,
            row.ExpenseCategoryName,
            row.SupplierUid,
            row.SupplierName,
            row.BookingUid,
            row.ExpenseDate,
            row.Description,
            row.Amount,
            row.Currency,
            row.PaymentMethod,
            row.ReferenceNumber,
            row.ReceiptUrl,
            row.Notes,
            ToDateTimeOffset(row.CreationDate))).ToList();
    }

    public async Task<ExpenseEntity?> GetByUidAsync(Guid expenseUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                e.id                    AS "Id",
                e.uid                   AS "Uid",
                e.organization_id       AS "OrganizationId",
                e.property_id           AS "PropertyId",
                p.uid                   AS "PropertyUid",
                e.expense_category_id   AS "ExpenseCategoryId",
                c.uid                   AS "ExpenseCategoryUid",
                e.supplier_id           AS "SupplierId",
                s.uid                   AS "SupplierUid",
                e.booking_id            AS "BookingId",
                bk.uid                  AS "BookingUid",
                e.expense_date          AS "ExpenseDate",
                e.description           AS "Description",
                e.amount                AS "Amount",
                rtrim(e.currency)       AS "Currency",
                e.payment_method        AS "PaymentMethod",
                e.reference_number      AS "ReferenceNumber",
                e.receipt_url           AS "ReceiptUrl",
                e.notes                 AS "Notes",
                e.is_active             AS "IsActive",
                e.is_archived           AS "IsArchived",
                e.creation_date         AS "CreationDate",
                e.created_by            AS "CreatedBy",
                e.modified_date         AS "ModifiedDate",
                e.modified_by           AS "ModifiedBy"
            FROM hotel.expenses e
            JOIN hotel.properties p ON p.id = e.property_id
            JOIN hotel.expense_categories c ON c.id = e.expense_category_id
            LEFT JOIN hotel.suppliers s ON s.id = e.supplier_id
            LEFT JOIN hotel.bookings bk ON bk.id = e.booking_id
            WHERE e.uid = @ExpenseUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ExpenseHeaderRow>(
            new CommandDefinition(sql, new { ExpenseUid = expenseUid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDomain(row);
    }

    public async Task<ExpenseDto?> GetDetailByUidAsync(Guid expenseUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                e.uid                   AS "Uid",
                p.uid                   AS "PropertyUid",
                c.uid                   AS "ExpenseCategoryUid",
                c.name                  AS "ExpenseCategoryName",
                s.uid                   AS "SupplierUid",
                s.name                  AS "SupplierName",
                b.uid                   AS "BookingUid",
                e.expense_date          AS "ExpenseDate",
                e.description           AS "Description",
                e.amount                AS "Amount",
                rtrim(e.currency)       AS "Currency",
                e.payment_method        AS "PaymentMethod",
                e.reference_number      AS "ReferenceNumber",
                e.receipt_url           AS "ReceiptUrl",
                e.notes                 AS "Notes",
                e.creation_date         AS "CreationDate"
            FROM hotel.expenses e
            JOIN hotel.properties p ON p.id = e.property_id
            JOIN hotel.expense_categories c ON c.id = e.expense_category_id
            LEFT JOIN hotel.suppliers s ON s.id = e.supplier_id
            LEFT JOIN hotel.bookings b ON b.id = e.booking_id
            WHERE e.uid = @ExpenseUid
              AND e.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<ExpenseListRow>(
            new CommandDefinition(sql, new { ExpenseUid = expenseUid }, cancellationToken: cancellationToken));

        return row is null ? null : ToDto(row);
    }

    public async Task UpdateAsync(ExpenseEntity expense, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.expenses
            SET expense_category_id = @ExpenseCategoryId,
                supplier_id = @SupplierId,
                booking_id = @BookingId,
                expense_date = @ExpenseDate,
                description = @Description,
                amount = @Amount,
                currency = @Currency,
                payment_method = @PaymentMethod,
                reference_number = @ReferenceNumber,
                receipt_url = @ReceiptUrl,
                notes = @Notes,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE uid = @Uid
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                expense.Uid,
                expense.ExpenseCategoryId,
                expense.SupplierId,
                expense.BookingId,
                expense.ExpenseDate,
                expense.Description,
                expense.Amount,
                expense.Currency,
                PaymentMethod = expense.PaymentMethod?.ToDatabaseValue(),
                expense.ReferenceNumber,
                expense.ReceiptUrl,
                expense.Notes,
                expense.ModifiedDate,
                expense.ModifiedBy
            },
            cancellationToken: cancellationToken));
    }

    public async Task DeleteAsync(long expenseId, string actorSubject, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.expenses
            SET is_active = false,
                is_archived = true,
                modified_date = CURRENT_TIMESTAMP,
                modified_by = @ActorSubject
            WHERE id = @ExpenseId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { ExpenseId = expenseId, ActorSubject = actorSubject },
            cancellationToken: cancellationToken));
    }

    private static ExpenseDto ToDto(ExpenseListRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.ExpenseCategoryUid,
        row.ExpenseCategoryName,
        row.SupplierUid,
        row.SupplierName,
        row.BookingUid,
        row.ExpenseDate,
        row.Description,
        row.Amount,
        row.Currency,
        row.PaymentMethod,
        row.ReferenceNumber,
        row.ReceiptUrl,
        row.Notes,
        ToDateTimeOffset(row.CreationDate));

    private static ExpenseEntity ToDomain(ExpenseHeaderRow row) => ExpenseEntity.Rehydrate(
        row.Id,
        row.Uid,
        row.OrganizationId,
        row.PropertyId,
        row.PropertyUid,
        row.ExpenseCategoryId,
        row.ExpenseCategoryUid,
        row.SupplierId,
        row.SupplierUid,
        row.BookingId,
        row.BookingUid,
        row.ExpenseDate,
        row.Description,
        row.Amount,
        row.Currency,
        FinancePaymentMethodMapper.FromDatabaseValue(row.PaymentMethod),
        row.ReferenceNumber,
        row.ReceiptUrl,
        row.Notes,
        row.IsActive,
        row.IsArchived,
        ToDateTimeOffset(row.CreationDate),
        row.CreatedBy,
        ToDateTimeOffset(row.ModifiedDate),
        row.ModifiedBy);

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private static DateTimeOffset? ToDateTimeOffset(DateTime? value) =>
        value is null ? null : ToDateTimeOffset(value.Value);

    private sealed class ExpenseListRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid ExpenseCategoryUid { get; init; }
        public string ExpenseCategoryName { get; init; } = string.Empty;
        public Guid? SupplierUid { get; init; }
        public string? SupplierName { get; init; }
        public Guid? BookingUid { get; init; }
        public DateOnly ExpenseDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? ReceiptUrl { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }

    private sealed class ExpenseHeaderRow
    {
        public long Id { get; init; }
        public Guid Uid { get; init; }
        public long OrganizationId { get; init; }
        public long PropertyId { get; init; }
        public Guid PropertyUid { get; init; }
        public long ExpenseCategoryId { get; init; }
        public Guid ExpenseCategoryUid { get; init; }
        public long? SupplierId { get; init; }
        public Guid? SupplierUid { get; init; }
        public long? BookingId { get; init; }
        public Guid? BookingUid { get; init; }
        public DateOnly ExpenseDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string? PaymentMethod { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? ReceiptUrl { get; init; }
        public string? Notes { get; init; }
        public bool IsActive { get; init; }
        public bool IsArchived { get; init; }
        public DateTime CreationDate { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? ModifiedDate { get; init; }
        public string? ModifiedBy { get; init; }
    }
}

