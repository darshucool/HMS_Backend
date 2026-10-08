using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.Commands.CreateBookingChargeType;
using HMS.Modules.Payments.Application.DTOs;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;
using HMS.Modules.Payments.Domain.Enums;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.UpdateBookingChargeType;

public sealed record UpdateBookingChargeTypeCommand(
    Guid PropertyUid,
    Guid ChargeTypeUid,
    string Code,
    string Name,
    BookingChargeCategory Category,
    bool IsTaxable,
    decimal? DefaultPrice,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingChargeTypeDto>>;

public sealed class UpdateBookingChargeTypeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeTypeRepository chargeTypeRepository)
    : IRequestHandler<UpdateBookingChargeTypeCommand, PaymentResult<BookingChargeTypeDto>>
{
    public async Task<PaymentResult<BookingChargeTypeDto>> Handle(
        UpdateBookingChargeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var property = await ChargeTypeGuard.LoadProperty(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return PaymentResult<BookingChargeTypeDto>.From(property.Error);

        var current = await chargeTypeRepository.GetByUidAsync(
            request.ChargeTypeUid, property.Value!.Id, cancellationToken);
        if (current is null || current.IsArchived)
            return PaymentResult<BookingChargeTypeDto>.NotFound("Charge type was not found.");

        var validation = BookingChargeTypeRules.Validate(request.Code, request.Name, request.Category, request.DefaultPrice);
        if (validation is not null)
            return PaymentResult<BookingChargeTypeDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await chargeTypeRepository.CodeExistsAsync(property.Value.Id, code, current.Uid, cancellationToken))
            return PaymentResult<BookingChargeTypeDto>.Conflict("This charge type code already exists for the property.");

        await chargeTypeRepository.UpdateAsync(
            current.Id,
            code,
            request.Name.Trim(),
            request.Category.ToDatabaseValue(),
            request.IsTaxable,
            request.DefaultPrice,
            request.IsActive,
            request.ActorSubject,
            cancellationToken);

        var detail = await chargeTypeRepository.GetDetailAsync(
            current.Uid, property.Value.Id, property.Value.Uid, cancellationToken);
        return detail is null
            ? PaymentResult<BookingChargeTypeDto>.NotFound("Charge type was not found.")
            : PaymentResult<BookingChargeTypeDto>.Success(detail);
    }
}
