using HMS.Modules.Payments.Api.Contracts;
using HMS.Modules.Payments.Application.Commands.CreateBookingCharge;
using HMS.Modules.Payments.Application.Commands.CreateBookingChargeType;
using HMS.Modules.Payments.Application.Commands.CreateBookingPayment;
using HMS.Modules.Payments.Application.Commands.DeleteBookingChargeType;
using HMS.Modules.Payments.Application.Commands.UpdateBookingChargeType;
using HMS.Modules.Payments.Application.Queries.GetBookingCharges;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeType;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;
using HMS.Modules.Payments.Application.Queries.GetBookingFinancialSummary;
using HMS.Modules.Payments.Application.Queries.GetBookingInvoice;
using HMS.Modules.Payments.Application.Queries.GetBookingPayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/bookings")]
[Authorize]
public sealed class BookingAccountController(ISender sender) : ControllerBase
{
    [HttpGet("{bookingUid:guid}/financial-summary")]
    public async Task<IActionResult> GetFinancialSummary(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingFinancialSummaryQuery(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{bookingUid:guid}/invoice")]
    public async Task<IActionResult> GetInvoice(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingInvoiceQuery(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{bookingUid:guid}/charges")]
    public async Task<IActionResult> GetCharges(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingChargesQuery(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/charges")]
    public async Task<IActionResult> CreateCharge(
        Guid bookingUid,
        [FromBody] UpsertBookingChargeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBookingChargeCommand(
            bookingUid,
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

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("/api/v1/properties/{propertyUid:guid}/booking-charge-types")]
    public async Task<IActionResult> GetChargeTypes(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingChargeTypesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("/api/v1/properties/{propertyUid:guid}/booking-charge-types")]
    public async Task<IActionResult> CreateChargeType(
        Guid propertyUid,
        [FromBody] UpsertBookingChargeTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBookingChargeTypeCommand(
            propertyUid,
            request.Code,
            request.Name,
            request.Category,
            request.IsTaxable,
            request.DefaultPrice,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("/api/v1/properties/{propertyUid:guid}/booking-charge-types/{chargeTypeUid:guid}")]
    public async Task<IActionResult> GetChargeType(
        Guid propertyUid,
        Guid chargeTypeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingChargeTypeQuery(
            propertyUid,
            chargeTypeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("/api/v1/properties/{propertyUid:guid}/booking-charge-types/{chargeTypeUid:guid}")]
    public async Task<IActionResult> UpdateChargeType(
        Guid propertyUid,
        Guid chargeTypeUid,
        [FromBody] UpsertBookingChargeTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateBookingChargeTypeCommand(
            propertyUid,
            chargeTypeUid,
            request.Code,
            request.Name,
            request.Category,
            request.IsTaxable,
            request.DefaultPrice,
            request.IsActive,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("/api/v1/properties/{propertyUid:guid}/booking-charge-types/{chargeTypeUid:guid}")]
    public async Task<IActionResult> DeleteChargeType(
        Guid propertyUid,
        Guid chargeTypeUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookingChargeTypeCommand(
            propertyUid,
            chargeTypeUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : this.ToActionResult(result);
    }

    [HttpGet("{bookingUid:guid}/payments")]
    public async Task<IActionResult> GetPayments(
        Guid bookingUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingPaymentsQuery(
            bookingUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{bookingUid:guid}/payments")]
    public async Task<IActionResult> CreatePayment(
        Guid bookingUid,
        [FromBody] CreateBookingPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateBookingPaymentCommand(
            bookingUid,
            request.PaymentMethod,
            request.PaymentType,
            request.Amount,
            request.Currency,
            request.Status,
            request.ReferenceNumber,
            request.PaidAt,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
