using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingChargeType;

public sealed record GetBookingChargeTypeQuery(
    Guid PropertyUid,
    Guid ChargeTypeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingChargeTypeDto>>;

public sealed class GetBookingChargeTypeQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeTypeRepository chargeTypeRepository)
    : IRequestHandler<GetBookingChargeTypeQuery, PaymentResult<BookingChargeTypeDto>>
{
    public async Task<PaymentResult<BookingChargeTypeDto>> Handle(
        GetBookingChargeTypeQuery request,
        CancellationToken cancellationToken)
    {
        var property = await ChargeTypeGuard.LoadProperty(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (property.Error is not null)
            return PaymentResult<BookingChargeTypeDto>.From(property.Error);

        var detail = await chargeTypeRepository.GetDetailAsync(
            request.ChargeTypeUid, property.Value!.Id, property.Value.Uid, cancellationToken);

        return detail is null
            ? PaymentResult<BookingChargeTypeDto>.NotFound("Charge type was not found.")
            : PaymentResult<BookingChargeTypeDto>.Success(detail);
    }
}
