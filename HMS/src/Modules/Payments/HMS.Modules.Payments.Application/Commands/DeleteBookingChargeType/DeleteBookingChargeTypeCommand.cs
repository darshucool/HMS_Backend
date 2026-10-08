using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.DeleteBookingChargeType;

public sealed record DeleteBookingChargeTypeCommand(
    Guid PropertyUid,
    Guid ChargeTypeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<bool>>;

public sealed class DeleteBookingChargeTypeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeTypeRepository chargeTypeRepository)
    : IRequestHandler<DeleteBookingChargeTypeCommand, PaymentResult<bool>>
{
    public async Task<PaymentResult<bool>> Handle(
        DeleteBookingChargeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var property = await ChargeTypeGuard.LoadProperty(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return PaymentResult<bool>.From(property.Error);

        var current = await chargeTypeRepository.GetByUidAsync(
            request.ChargeTypeUid, property.Value!.Id, cancellationToken);
        if (current is null || current.IsArchived)
            return PaymentResult<bool>.NotFound("Charge type was not found.");

        await chargeTypeRepository.ArchiveAsync(current.Id, request.ActorSubject, cancellationToken);
        return PaymentResult<bool>.Success(true);
    }
}
