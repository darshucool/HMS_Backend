using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using MediatR;

namespace HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;

public sealed record GetBookingChargeTypesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<IReadOnlyList<BookingChargeTypeDto>>>;

public sealed class GetBookingChargeTypesQueryHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeTypeRepository chargeTypeRepository)
    : IRequestHandler<GetBookingChargeTypesQuery, PaymentResult<IReadOnlyList<BookingChargeTypeDto>>>
{
    public async Task<PaymentResult<IReadOnlyList<BookingChargeTypeDto>>> Handle(
        GetBookingChargeTypesQuery request,
        CancellationToken cancellationToken)
    {
        var property = await ChargeTypeGuard.LoadProperty(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (property.Error is not null)
            return PaymentResult<IReadOnlyList<BookingChargeTypeDto>>.From(property.Error);

        var items = await chargeTypeRepository.ListAsync(property.Value!.Id, property.Value.Uid, cancellationToken);
        return PaymentResult<IReadOnlyList<BookingChargeTypeDto>>.Success(items);
    }
}

internal static class ChargeTypeGuard
{
    public static async Task<(PaymentPropertyContext? Value, PaymentResult<object>? Error)> LoadProperty(
        IPaymentPropertyAccess propertyAccess,
        Guid propertyUid,
        string actorSubject,
        bool isPlatformAdmin,
        bool requireManager,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetByUidAsync(propertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return (null, PaymentResult<object>.NotFound("Property was not found."));

        if (!isPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(actorSubject, propertyUid, requireManager, cancellationToken))
        {
            return (null, PaymentResult<object>.Forbidden(
                requireManager
                    ? "You cannot change charge types for this property."
                    : "You cannot view charge types for this property."));
        }

        return (property, null);
    }
}
