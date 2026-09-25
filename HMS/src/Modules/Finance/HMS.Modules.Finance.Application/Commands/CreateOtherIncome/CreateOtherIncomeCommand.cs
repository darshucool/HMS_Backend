using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using MediatR;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Application.Commands.CreateOtherIncome;

public sealed record CreateOtherIncomeCommand(
    Guid PropertyUid,
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

public sealed class CreateOtherIncomeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IOtherIncomeRepository otherIncomeRepository)
    : IRequestHandler<CreateOtherIncomeCommand, FinanceResult<OtherIncomeDto>>
{
    public async Task<FinanceResult<OtherIncomeDto>> Handle(
        CreateOtherIncomeCommand request,
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
                "You cannot create other income for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<OtherIncomeDto>.NotFound("Property was not found.");

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

        OtherIncomeEntity income;
        try
        {
            income = OtherIncomeEntity.Create(
                property.OrganizationId,
                property.Id,
                request.PropertyUid,
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

        await otherIncomeRepository.InsertAsync(income, cancellationToken);

        return FinanceResult<OtherIncomeDto>.Success(new OtherIncomeDto(
            income.Uid,
            income.PropertyUid,
            income.IncomeCategoryUid,
            category.Name,
            income.BookingUid,
            income.IncomeDate,
            income.Description,
            income.Amount,
            income.Currency,
            income.PaymentMethod?.ToDatabaseValue(),
            income.ReferenceNumber,
            income.Notes,
            income.CreationDate));
    }
}
