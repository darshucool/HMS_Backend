using HMS.Modules.Payments.Api.Contracts;
using HMS.Modules.Payments.Application.Commands.RefundBookingPayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/booking-payments")]
[Authorize]
public sealed class BookingPaymentsController(ISender sender) : ControllerBase
{
    [HttpPost("{paymentUid:guid}/refund")]
    public async Task<IActionResult> Refund(
        Guid paymentUid,
        [FromBody] RefundBookingPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefundBookingPaymentCommand(
            paymentUid,
            request.Amount,
            request.Reason,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
