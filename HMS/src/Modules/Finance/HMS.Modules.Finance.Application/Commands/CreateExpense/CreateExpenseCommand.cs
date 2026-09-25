using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using MediatR;
using ExpenseEntity = HMS.Modules.Finance.Domain.Entities.Expense;

namespace HMS.Modules.Finance.Application.Commands.CreateExpense;

public sealed record CreateExpenseCommand(
    Guid PropertyUid,
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

public sealed class CreateExpenseCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseRepository expenseRepository)
    : IRequestHandler<CreateExpenseCommand, FinanceResult<ExpenseDto>>
{
    public async Task<FinanceResult<ExpenseDto>> Handle(
        CreateExpenseCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return FinanceResult<ExpenseDto>.Forbidden("You cannot create expenses for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<ExpenseDto>.NotFound("Property was not found.");

        var category = await expenseRepository.GetCategoryAsync(request.ExpenseCategoryUid, cancellationToken);
        if (category is null || category.OrganizationId != property.OrganizationId)
            return FinanceResult<ExpenseDto>.NotFound("Expense category was not found.");

        SupplierRef? supplier = null;
        if (request.SupplierUid is Guid supplierUid)
        {
            supplier = await expenseRepository.GetSupplierAsync(supplierUid, cancellationToken);
            if (supplier is null || supplier.OrganizationId != property.OrganizationId)
                return FinanceResult<ExpenseDto>.NotFound("Supplier was not found.");
        }

        BookingRef? booking = null;
        if (request.BookingUid is Guid bookingUid)
        {
            booking = await expenseRepository.GetBookingAsync(bookingUid, cancellationToken);
            if (booking is null || booking.PropertyId != property.Id)
                return FinanceResult<ExpenseDto>.NotFound("Booking was not found for this property.");
        }

        ExpenseEntity expense;
        try
        {
            expense = ExpenseEntity.Create(
                property.OrganizationId,
                property.Id,
                request.PropertyUid,
                category.Id,
                category.Uid,
                supplier?.Id,
                supplier?.Uid,
                booking?.Id,
                booking?.Uid,
                request.ExpenseDate,
                request.Description,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.ReceiptUrl,
                request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return FinanceResult<ExpenseDto>.Validation(exception.Message);
        }

        await expenseRepository.InsertAsync(expense, cancellationToken);

        return FinanceResult<ExpenseDto>.Success(new ExpenseDto(
            expense.Uid,
            expense.PropertyUid,
            expense.ExpenseCategoryUid,
            category.Name,
            expense.SupplierUid,
            supplier?.Name,
            expense.BookingUid,
            expense.ExpenseDate,
            expense.Description,
            expense.Amount,
            expense.Currency,
            expense.PaymentMethod?.ToDatabaseValue(),
            expense.ReferenceNumber,
            expense.ReceiptUrl,
            expense.Notes,
            expense.CreationDate));
    }
}
