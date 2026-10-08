using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(
    Guid ExpenseUid,
    Guid ExpenseCategoryUid,
    Guid? SupplierUid,
    Guid? BookingUid,
    DateOnly ExpenseDate,
    string Description,
    decimal Amount,
    string Currency,
    FinancePaymentMethod? PaymentMethod,
    string? ReferenceNumber,
    string? ReceiptUrl,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<ExpenseDto>>;

public sealed class UpdateExpenseCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseRepository expenseRepository)
    : IRequestHandler<UpdateExpenseCommand, FinanceResult<ExpenseDto>>
{
    public async Task<FinanceResult<ExpenseDto>> Handle(
        UpdateExpenseCommand request,
        CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByUidAsync(request.ExpenseUid, cancellationToken);
        if (expense is null || expense.IsArchived)
            return FinanceResult<ExpenseDto>.NotFound("Expense was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject, expense.PropertyUid, true, cancellationToken))
        {
            return FinanceResult<ExpenseDto>.Forbidden("You cannot modify this expense.");
        }

        var category = await expenseRepository.GetCategoryAsync(request.ExpenseCategoryUid, cancellationToken);
        if (category is null || category.OrganizationId != expense.OrganizationId)
            return FinanceResult<ExpenseDto>.Validation("Expense category was not found.");

        long? supplierId = null;
        if (request.SupplierUid is Guid supplierUid)
        {
            var supplier = await expenseRepository.GetSupplierAsync(supplierUid, cancellationToken);
            if (supplier is null || supplier.OrganizationId != expense.OrganizationId)
                return FinanceResult<ExpenseDto>.Validation("Supplier was not found.");

            supplierId = supplier.Id;
        }

        long? bookingId = null;
        if (request.BookingUid is Guid bookingUid)
        {
            var booking = await expenseRepository.GetBookingAsync(bookingUid, cancellationToken);
            if (booking is null || booking.OrganizationId != expense.OrganizationId ||
                booking.PropertyId != expense.PropertyId)
            {
                return FinanceResult<ExpenseDto>.Validation("Booking was not found for this property.");
            }

            bookingId = booking.Id;
        }

        try
        {
            expense.Update(
                category.Id, request.ExpenseCategoryUid,
                supplierId, request.SupplierUid,
                bookingId, request.BookingUid,
                request.ExpenseDate, request.Description, request.Amount, request.Currency,
                request.PaymentMethod, request.ReferenceNumber, request.ReceiptUrl, request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return FinanceResult<ExpenseDto>.Validation(exception.Message);
        }

        await expenseRepository.UpdateAsync(expense, cancellationToken);

        var detail = await expenseRepository.GetDetailByUidAsync(request.ExpenseUid, cancellationToken);
        return detail is null
            ? FinanceResult<ExpenseDto>.NotFound("Expense was not found.")
            : FinanceResult<ExpenseDto>.Success(detail);
    }
}