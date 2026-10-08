using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using MediatR;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Application.Commands.UpdateOtherIncome;

public sealed record UpdateOtherIncomeCommand(
    Guid PropertyUid,
    Guid OtherIncomeUid,
    Guid IncomeCategoryUid,
    Guid? BookingUid,
    DateOnly IncomeDate,
    string Description,
    decimal Amount,
    string Currency,
    FinancePaymentMethod? PaymentMethod,
    string? ReferenceNumber,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<OtherIncomeDto>>;

public sealed class UpdateOtherIncomeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IOtherIncomeRepository otherIncomeRepository)
    : IRequestHandler<UpdateOtherIncomeCommand, FinanceResult<OtherIncomeDto>>
{
    public async Task<FinanceResult<OtherIncomeDto>> Handle(
        UpdateOtherIncomeCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return FinanceResult<OtherIncomeDto>.Forbidden(
                "You cannot update other income for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<OtherIncomeDto>.NotFound("Property was not found.");

        var current = await otherIncomeRepository.GetContextAsync(
            request.PropertyUid,
            request.OtherIncomeUid,
            cancellationToken);
        if (current is null || current.IsArchived)
            return FinanceResult<OtherIncomeDto>.NotFound("Other income was not found.");

        var category = await otherIncomeRepository.GetCategoryAsync(request.IncomeCategoryUid, cancellationToken);
        if (category is null || category.OrganizationId != property.OrganizationId)
            return FinanceResult<OtherIncomeDto>.NotFound("Income category was not found.");

        BookingRef? booking = null;
        if (request.BookingUid is Guid bookingUid)
        {
            booking = await otherIncomeRepository.GetBookingAsync(bookingUid, cancellationToken);
            if (booking is null || booking.PropertyId != property.Id)
                return FinanceResult<OtherIncomeDto>.NotFound("Booking was not found for this property.");
        }

        var income = OtherIncomeEntity.Rehydrate(
            current.Id,
            current.Uid,
            current.OrganizationId,
            current.PropertyId,
            current.PropertyUid,
            current.IsActive,
            current.IsArchived,
            current.CreationDate,
            current.CreatedBy);

        try
        {
            income.Update(
                category.Id,
                category.Uid,
                booking?.Id,
                booking?.Uid,
                request.IncomeDate,
                request.Description,
                request.Amount,
                request.Currency,
                request.PaymentMethod,
                request.ReferenceNumber,
                request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return FinanceResult<OtherIncomeDto>.Validation(exception.Message);
        }

        await otherIncomeRepository.UpdateAsync(income, cancellationToken);

        var detail = await otherIncomeRepository.GetByUidAsync(
            request.PropertyUid,
            income.Uid,
            cancellationToken);

        return detail is null
            ? FinanceResult<OtherIncomeDto>.NotFound("Other income was not found.")
            : FinanceResult<OtherIncomeDto>.Success(detail);
    }
}
