using HMS.Modules.Payments.Api.Contracts;
using HMS.Modules.Payments.Application.Commands.DeleteBookingCharge;
using HMS.Modules.Payments.Application.Commands.UpdateBookingCharge;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/booking-charges")]
[Authorize]
public sealed class BookingChargesController(ISender sender) : ControllerBase
{
    [HttpPut("{chargeUid:guid}")]
    public async Task<IActionResult> Update(
        Guid chargeUid,
        [FromBody] UpsertBookingChargeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateBookingChargeCommand(
            chargeUid,
            request.ChargeTypeUid,
            request.BookingUnitUid,
            request.ServiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.DiscountAmount,
            request.TaxAmount,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{chargeUid:guid}")]
    public async Task<IActionResult> Delete(
        Guid chargeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookingChargeCommand(
            chargeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }
}
