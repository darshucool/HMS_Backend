using HMS.Modules.Finance.Application.DTOs;
using ExpenseEntity = HMS.Modules.Finance.Domain.Entities.Expense;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record FinancePropertyContext(long Id, long OrganizationId, bool IsArchived);

public sealed record ExpenseCategoryRef(long Id, Guid Uid, string Name, long OrganizationId);

public sealed record SupplierRef(long Id, Guid Uid, string Name, long OrganizationId);

public sealed record BookingRef(long Id, Guid Uid, long OrganizationId, long PropertyId);

public interface IFinancePropertyAccess
{
    Task<FinancePropertyContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
}

public interface IExpenseRepository
{
    Task<ExpenseCategoryRef?> GetCategoryAsync(Guid categoryUid, CancellationToken cancellationToken);
    Task<SupplierRef?> GetSupplierAsync(Guid supplierUid, CancellationToken cancellationToken);
    Task<BookingRef?> GetBookingAsync(Guid bookingUid, CancellationToken cancellationToken);
    Task InsertAsync(ExpenseEntity expense, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExpenseDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
    Task<ExpenseEntity?> GetByUidAsync(Guid expenseUid, CancellationToken cancellationToken);
    Task<ExpenseDto?> GetDetailByUidAsync(Guid expenseUid, CancellationToken cancellationToken);
    Task UpdateAsync(ExpenseEntity expense, CancellationToken cancellationToken);
    Task DeleteAsync(long expenseId, string actorSubject, CancellationToken cancellationToken);
}
