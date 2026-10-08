using Dapper;
using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.DTOs;

namespace HMS.Modules.Staff.Infrastructure.Persistence.Repositories;

public sealed class StaffPropertyAccessRepository(IStaffDbConnectionFactory connectionFactory)
    : IStaffPropertyAccess
{
    public async Task<StaffPropertyContext?> GetPropertyAsync(Guid propertyUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                organization_id AS "OrganizationId",
                uid             AS "Uid",
                is_archived     AS "IsArchived"
            FROM hotel.properties
            WHERE uid = @PropertyUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<StaffPropertyContext>(
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
            new { ActorSubject = actorSubject, PropertyUid = propertyUid, RequireManager = requireManager },
            cancellationToken: cancellationToken));
    }
}

public sealed class StaffRepository(IStaffDbConnectionFactory connectionFactory) : IStaffRepository
{
    private const string StaffSelect = """
        SELECT
            s.uid               AS "Uid",
            p.uid               AS "PropertyUid",
            r.uid               AS "StaffRoleUid",
            r.name              AS "StaffRoleName",
            s.employee_number   AS "EmployeeNumber",
            s.first_name        AS "FirstName",
            s.last_name         AS "LastName",
            s.phone             AS "Phone",
            s.email             AS "Email",
            s.employment_type   AS "EmploymentType",
            s.basic_salary      AS "BasicSalary",
            s.daily_rate        AS "DailyRate",
            s.hourly_rate       AS "HourlyRate",
            s.joined_date       AS "JoinedDate",
            s.left_date         AS "LeftDate",
            s.status            AS "Status",
            s.creation_date     AS "CreationDate"
        FROM hotel.staff_members s
        JOIN hotel.properties p ON p.id = s.property_id
        JOIN hotel.staff_roles r ON r.id = s.staff_role_id
        """;

    public async Task<StaffRoleRef?> GetRoleAsync(Guid staffRoleUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                id              AS "Id",
                organization_id AS "OrganizationId",
                name            AS "Name"
            FROM hotel.staff_roles
            WHERE uid = @StaffRoleUid
              AND is_archived = false
              AND is_active = true;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<StaffRoleRef>(
            new CommandDefinition(sql, new { StaffRoleUid = staffRoleUid }, cancellationToken: cancellationToken));
    }

    public async Task<long?> GetBookingIdAsync(Guid bookingUid, long propertyId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id
            FROM hotel.bookings
            WHERE uid = @BookingUid
              AND property_id = @PropertyId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<long?>(
            new CommandDefinition(
                sql,
                new { BookingUid = bookingUid, PropertyId = propertyId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<StaffMemberDto>> GetByPropertyAsync(long propertyId, CancellationToken cancellationToken)
    {
        var sql = $"""
            {StaffSelect}
            WHERE s.property_id = @PropertyId
              AND s.is_archived = false
            ORDER BY s.first_name, s.last_name, s.employee_number;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<StaffRow>(
            new CommandDefinition(sql, new { PropertyId = propertyId }, cancellationToken: cancellationToken));
        return rows.Select(ToStaff).ToList();
    }

    public async Task<StaffMemberContext?> GetContextAsync(Guid staffUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                s.id                AS "Id",
                s.uid               AS "Uid",
                s.organization_id   AS "OrganizationId",
                s.property_id       AS "PropertyId",
                p.uid               AS "PropertyUid",
                s.is_archived       AS "IsArchived"
            FROM hotel.staff_members s
            JOIN hotel.properties p ON p.id = s.property_id
            WHERE s.uid = @StaffUid;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<StaffMemberContext>(
            new CommandDefinition(sql, new { StaffUid = staffUid }, cancellationToken: cancellationToken));
    }

    public async Task<StaffMemberDto?> GetDetailAsync(Guid staffUid, CancellationToken cancellationToken)
    {
        var sql = $"""
            {StaffSelect}
            WHERE s.uid = @StaffUid
              AND s.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<StaffRow>(
            new CommandDefinition(sql, new { StaffUid = staffUid }, cancellationToken: cancellationToken));
        return row is null ? null : ToStaff(row);
    }

    public async Task<bool> EmployeeNumberExistsAsync(
        long propertyId,
        string employeeNumber,
        long? excludeStaffId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM hotel.staff_members
                WHERE property_id = @PropertyId
                  AND employee_number = @EmployeeNumber
                  AND is_archived = false
                  AND (@ExcludeStaffId IS NULL OR id <> @ExcludeStaffId)
            );
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            sql,
            new { PropertyId = propertyId, EmployeeNumber = employeeNumber, ExcludeStaffId = excludeStaffId },
            cancellationToken: cancellationToken));
    }

    public async Task<Guid> InsertAsync(
        long organizationId,
        long propertyId,
        long staffRoleId,
        string employeeNumber,
        string firstName,
        string? lastName,
        string? phone,
        string? email,
        string employmentType,
        decimal? basicSalary,
        decimal? dailyRate,
        decimal? hourlyRate,
        DateOnly? joinedDate,
        DateOnly? leftDate,
        string status,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.staff_members
            (
                uid, organization_id, property_id, staff_role_id, employee_number,
                first_name, last_name, phone, email, employment_type,
                basic_salary, daily_rate, hourly_rate, joined_date, left_date, status,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @StaffRoleId, @EmployeeNumber,
                @FirstName, @LastName, @Phone, @Email, @EmploymentType,
                @BasicSalary, @DailyRate, @HourlyRate, @JoinedDate, @LeftDate, @Status,
                true, false, @CreationDate, @CreatedBy
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
                StaffRoleId = staffRoleId,
                EmployeeNumber = employeeNumber,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email,
                EmploymentType = employmentType,
                BasicSalary = basicSalary,
                DailyRate = dailyRate,
                HourlyRate = hourlyRate,
                JoinedDate = joinedDate,
                LeftDate = leftDate,
                Status = status,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task UpdateAsync(
        long staffId,
        long staffRoleId,
        string employeeNumber,
        string firstName,
        string? lastName,
        string? phone,
        string? email,
        string employmentType,
        decimal? basicSalary,
        decimal? dailyRate,
        decimal? hourlyRate,
        DateOnly? joinedDate,
        DateOnly? leftDate,
        string status,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE hotel.staff_members
            SET staff_role_id = @StaffRoleId,
                employee_number = @EmployeeNumber,
                first_name = @FirstName,
                last_name = @LastName,
                phone = @Phone,
                email = @Email,
                employment_type = @EmploymentType,
                basic_salary = @BasicSalary,
                daily_rate = @DailyRate,
                hourly_rate = @HourlyRate,
                joined_date = @JoinedDate,
                left_date = @LeftDate,
                status = @Status,
                modified_date = @ModifiedDate,
                modified_by = @ModifiedBy
            WHERE id = @StaffId
              AND is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                StaffId = staffId,
                StaffRoleId = staffRoleId,
                EmployeeNumber = employeeNumber,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Email = email,
                EmploymentType = employmentType,
                BasicSalary = basicSalary,
                DailyRate = dailyRate,
                HourlyRate = hourlyRate,
                JoinedDate = joinedDate,
                LeftDate = leftDate,
                Status = status,
                ModifiedDate = DateTimeOffset.UtcNow,
                ModifiedBy = actorSubject
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<StaffWorkLogDto>> GetWorkLogsAsync(long staffId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                w.uid               AS "Uid",
                s.uid               AS "StaffUid",
                b.uid               AS "BookingUid",
                w.work_date         AS "WorkDate",
                w.start_time        AS "StartTime",
                w.end_time          AS "EndTime",
                w.hours_worked      AS "HoursWorked",
                w.overtime_hours    AS "OvertimeHours",
                w.notes             AS "Notes",
                w.creation_date     AS "CreationDate"
            FROM hotel.staff_work_logs w
            JOIN hotel.staff_members s ON s.id = w.staff_id
            LEFT JOIN hotel.bookings b ON b.id = w.booking_id
            WHERE w.staff_id = @StaffId
              AND w.is_archived = false
            ORDER BY w.work_date DESC, w.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<WorkLogRow>(
            new CommandDefinition(sql, new { StaffId = staffId }, cancellationToken: cancellationToken));
        return rows.Select(ToWorkLog).ToList();
    }

    public async Task<Guid> InsertWorkLogAsync(
        long organizationId,
        long propertyId,
        long staffId,
        long? bookingId,
        DateOnly workDate,
        TimeOnly? startTime,
        TimeOnly? endTime,
        decimal hoursWorked,
        decimal overtimeHours,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.staff_work_logs
            (
                uid, organization_id, property_id, staff_id, booking_id,
                work_date, start_time, end_time, hours_worked, overtime_hours, notes,
                is_active, is_archived, creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @StaffId, @BookingId,
                @WorkDate, @StartTime, @EndTime, @HoursWorked, @OvertimeHours, @Notes,
                true, false, @CreationDate, @CreatedBy
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
                StaffId = staffId,
                BookingId = bookingId,
                WorkDate = workDate,
                StartTime = startTime,
                EndTime = endTime,
                HoursWorked = hoursWorked,
                OvertimeHours = overtimeHours,
                Notes = notes,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task<StaffWorkLogDto?> GetWorkLogAsync(Guid workLogUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                w.uid               AS "Uid",
                s.uid               AS "StaffUid",
                b.uid               AS "BookingUid",
                w.work_date         AS "WorkDate",
                w.start_time        AS "StartTime",
                w.end_time          AS "EndTime",
                w.hours_worked      AS "HoursWorked",
                w.overtime_hours    AS "OvertimeHours",
                w.notes             AS "Notes",
                w.creation_date     AS "CreationDate"
            FROM hotel.staff_work_logs w
            JOIN hotel.staff_members s ON s.id = w.staff_id
            LEFT JOIN hotel.bookings b ON b.id = w.booking_id
            WHERE w.uid = @WorkLogUid
              AND w.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<WorkLogRow>(
            new CommandDefinition(sql, new { WorkLogUid = workLogUid }, cancellationToken: cancellationToken));
        return row is null ? null : ToWorkLog(row);
    }

    public async Task<IReadOnlyList<StaffPaymentDto>> GetPaymentsByPropertyAsync(
        long propertyId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pay.uid                 AS "Uid",
                p.uid                   AS "PropertyUid",
                s.uid                   AS "StaffUid",
                s.employee_number       AS "EmployeeNumber",
                trim(both ' ' from concat(s.first_name, ' ', coalesce(s.last_name, ''))) AS "StaffName",
                pay.period_start        AS "PeriodStart",
                pay.period_end          AS "PeriodEnd",
                pay.basic_amount        AS "BasicAmount",
                pay.overtime_amount     AS "OvertimeAmount",
                pay.bonus_amount        AS "BonusAmount",
                pay.deduction_amount    AS "DeductionAmount",
                pay.total_paid          AS "TotalPaid",
                pay.payment_method      AS "PaymentMethod",
                pay.paid_at             AS "PaidAt",
                pay.reference_number    AS "ReferenceNumber",
                pay.notes               AS "Notes",
                pay.creation_date       AS "CreationDate"
            FROM hotel.staff_payments pay
            JOIN hotel.staff_members s ON s.id = pay.staff_id
            JOIN hotel.properties p ON p.id = pay.property_id
            WHERE pay.property_id = @PropertyId
              AND pay.is_archived = false
            ORDER BY pay.paid_at DESC, pay.creation_date DESC;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<PaymentRow>(
            new CommandDefinition(sql, new { PropertyId = propertyId }, cancellationToken: cancellationToken));
        return rows.Select(ToPayment).ToList();
    }

    public async Task<Guid> InsertPaymentAsync(
        long organizationId,
        long propertyId,
        long staffId,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal basicAmount,
        decimal overtimeAmount,
        decimal bonusAmount,
        decimal deductionAmount,
        decimal totalPaid,
        string paymentMethod,
        DateTimeOffset paidAt,
        string? referenceNumber,
        string? notes,
        string actorSubject,
        CancellationToken cancellationToken)
    {
        var uid = Guid.NewGuid();
        const string sql = """
            INSERT INTO hotel.staff_payments
            (
                uid, organization_id, property_id, staff_id,
                period_start, period_end, basic_amount, overtime_amount,
                bonus_amount, deduction_amount, total_paid, payment_method,
                paid_at, reference_number, notes, is_active, is_archived,
                creation_date, created_by
            )
            VALUES
            (
                @Uid, @OrganizationId, @PropertyId, @StaffId,
                @PeriodStart, @PeriodEnd, @BasicAmount, @OvertimeAmount,
                @BonusAmount, @DeductionAmount, @TotalPaid, @PaymentMethod,
                @PaidAt, @ReferenceNumber, @Notes, true, false,
                @CreationDate, @CreatedBy
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
                StaffId = staffId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BasicAmount = basicAmount,
                OvertimeAmount = overtimeAmount,
                BonusAmount = bonusAmount,
                DeductionAmount = deductionAmount,
                TotalPaid = totalPaid,
                PaymentMethod = paymentMethod,
                PaidAt = paidAt,
                ReferenceNumber = referenceNumber,
                Notes = notes,
                CreationDate = DateTimeOffset.UtcNow,
                CreatedBy = actorSubject
            },
            cancellationToken: cancellationToken));

        return uid;
    }

    public async Task<StaffPaymentDto?> GetPaymentAsync(Guid paymentUid, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pay.uid                 AS "Uid",
                p.uid                   AS "PropertyUid",
                s.uid                   AS "StaffUid",
                s.employee_number       AS "EmployeeNumber",
                trim(both ' ' from concat(s.first_name, ' ', coalesce(s.last_name, ''))) AS "StaffName",
                pay.period_start        AS "PeriodStart",
                pay.period_end          AS "PeriodEnd",
                pay.basic_amount        AS "BasicAmount",
                pay.overtime_amount     AS "OvertimeAmount",
                pay.bonus_amount        AS "BonusAmount",
                pay.deduction_amount    AS "DeductionAmount",
                pay.total_paid          AS "TotalPaid",
                pay.payment_method      AS "PaymentMethod",
                pay.paid_at             AS "PaidAt",
                pay.reference_number    AS "ReferenceNumber",
                pay.notes               AS "Notes",
                pay.creation_date       AS "CreationDate"
            FROM hotel.staff_payments pay
            JOIN hotel.staff_members s ON s.id = pay.staff_id
            JOIN hotel.properties p ON p.id = pay.property_id
            WHERE pay.uid = @PaymentUid
              AND pay.is_archived = false;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<PaymentRow>(
            new CommandDefinition(sql, new { PaymentUid = paymentUid }, cancellationToken: cancellationToken));
        return row is null ? null : ToPayment(row);
    }

    private static StaffMemberDto ToStaff(StaffRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.StaffRoleUid,
        row.StaffRoleName,
        row.EmployeeNumber,
        row.FirstName,
        row.LastName,
        row.Phone,
        row.Email,
        row.EmploymentType,
        row.BasicSalary,
        row.DailyRate,
        row.HourlyRate,
        row.JoinedDate,
        row.LeftDate,
        row.Status,
        ToDateTimeOffset(row.CreationDate));

    private static StaffWorkLogDto ToWorkLog(WorkLogRow row) => new(
        row.Uid,
        row.StaffUid,
        row.BookingUid,
        row.WorkDate,
        row.StartTime,
        row.EndTime,
        row.HoursWorked,
        row.OvertimeHours,
        row.Notes,
        ToDateTimeOffset(row.CreationDate));

    private static StaffPaymentDto ToPayment(PaymentRow row) => new(
        row.Uid,
        row.PropertyUid,
        row.StaffUid,
        row.EmployeeNumber,
        row.StaffName.Trim(),
        row.PeriodStart,
        row.PeriodEnd,
        row.BasicAmount,
        row.OvertimeAmount,
        row.BonusAmount,
        row.DeductionAmount,
        row.TotalPaid,
        row.PaymentMethod,
        ToDateTimeOffset(row.PaidAt),
        row.ReferenceNumber,
        row.Notes,
        ToDateTimeOffset(row.CreationDate));

    private static DateTimeOffset ToDateTimeOffset(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
            : new DateTimeOffset(value);

    private sealed class StaffRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid StaffRoleUid { get; init; }
        public string StaffRoleName { get; init; } = string.Empty;
        public string EmployeeNumber { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string? LastName { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }
        public string EmploymentType { get; init; } = string.Empty;
        public decimal? BasicSalary { get; init; }
        public decimal? DailyRate { get; init; }
        public decimal? HourlyRate { get; init; }
        public DateOnly? JoinedDate { get; init; }
        public DateOnly? LeftDate { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreationDate { get; init; }
    }

    private sealed class WorkLogRow
    {
        public Guid Uid { get; init; }
        public Guid StaffUid { get; init; }
        public Guid? BookingUid { get; init; }
        public DateOnly WorkDate { get; init; }
        public TimeOnly? StartTime { get; init; }
        public TimeOnly? EndTime { get; init; }
        public decimal HoursWorked { get; init; }
        public decimal OvertimeHours { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }

    private sealed class PaymentRow
    {
        public Guid Uid { get; init; }
        public Guid PropertyUid { get; init; }
        public Guid StaffUid { get; init; }
        public string EmployeeNumber { get; init; } = string.Empty;
        public string StaffName { get; init; } = string.Empty;
        public DateOnly PeriodStart { get; init; }
        public DateOnly PeriodEnd { get; init; }
        public decimal BasicAmount { get; init; }
        public decimal OvertimeAmount { get; init; }
        public decimal BonusAmount { get; init; }
        public decimal DeductionAmount { get; init; }
        public decimal TotalPaid { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public DateTime PaidAt { get; init; }
        public string? ReferenceNumber { get; init; }
        public string? Notes { get; init; }
        public DateTime CreationDate { get; init; }
    }
}
