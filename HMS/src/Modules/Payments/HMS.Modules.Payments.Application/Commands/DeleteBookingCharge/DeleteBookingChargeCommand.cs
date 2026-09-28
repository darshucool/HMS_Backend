using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.DeleteBookingCharge;

public sealed record DeleteBookingChargeCommand(
    Guid ChargeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<bool>>;

public sealed class DeleteBookingChargeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeRepository chargeRepository)
    : IRequestHandler<DeleteBookingChargeCommand, PaymentResult<bool>>
{
    public async Task<PaymentResult<bool>> Handle(
        DeleteBookingChargeCommand request,
        CancellationToken cancellationToken)
    {
        var charge = await chargeRepository.GetByUidAsync(request.ChargeUid, cancellationToken);
        if (charge is null || charge.IsArchived)
            return PaymentResult<bool>.NotFound("Charge was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, charge.PropertyUid, true, cancellationToken))
        {
            return PaymentResult<bool>.Forbidden("You cannot delete this charge.");
        }

        await chargeRepository.DeleteAsync(charge.Id, request.ActorSubject, cancellationToken);
        return PaymentResult<bool>.Success(true);
    }
}
