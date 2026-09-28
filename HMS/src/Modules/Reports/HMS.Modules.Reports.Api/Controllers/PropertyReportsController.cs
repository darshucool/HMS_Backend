using HMS.Modules.Reports.Application.Queries.GetBookingProfitability;
using HMS.Modules.Reports.Application.Queries.GetBookingRevenue;
using HMS.Modules.Reports.Application.Queries.GetExpenseReport;
using HMS.Modules.Reports.Application.Queries.GetGuestProfitability;
using HMS.Modules.Reports.Application.Queries.GetMonthlyPropertySummary;
using HMS.Modules.Reports.Application.Queries.GetOccupancy;
using HMS.Modules.Reports.Application.Queries.GetOutstandingBalances;
using HMS.Modules.Reports.Application.Queries.GetPaymentSummary;
using HMS.Modules.Reports.Application.Queries.GetUtilityReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Reports.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}/reports")]
[Authorize]
public sealed class PropertyReportsController(ISender sender) : ControllerBase
{
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary(
        Guid propertyUid,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMonthlyPropertySummaryQuery(
            propertyUid,
            year,
            month,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("booking-revenue")]
    public async Task<IActionResult> GetBookingRevenue(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingRevenueQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("booking-profitability")]
    public async Task<IActionResult> GetBookingProfitability(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookingProfitabilityQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("guest-profitability")]
    public async Task<IActionResult> GetGuestProfitability(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGuestProfitabilityQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOccupancyQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("payment-summary")]
    public async Task<IActionResult> GetPaymentSummary(
        Guid propertyUid,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPaymentSummaryQuery(
            propertyUid,
            year,
            month,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("outstanding-balances")]
    public async Task<IActionResult> GetOutstandingBalances(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetOutstandingBalancesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("expenses")]
    public async Task<IActionResult> GetExpenses(
        Guid propertyUid,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExpenseReportQuery(
            propertyUid,
            year,
            month,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("utilities")]
    public async Task<IActionResult> GetUtilities(
        Guid propertyUid,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUtilityReportQuery(
            propertyUid,
            year,
            month,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
