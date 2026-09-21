using HMS.Modules.Hotels.Api.Contracts;
using HMS.Modules.Hotels.Application.Commands.CreateAccommodationType;
using HMS.Modules.Hotels.Application.Commands.CreateAccommodationUnit;
using HMS.Modules.Hotels.Application.Commands.CreateMealPlan;
using HMS.Modules.Hotels.Application.Commands.CreateRatePlan;
using HMS.Modules.Hotels.Application.Commands.UpdateProperty;
using HMS.Modules.Hotels.Application.Commands.UpdatePropertySettings;
using HMS.Modules.Hotels.Application.Queries.GetAccommodationTypes;
using HMS.Modules.Hotels.Application.Queries.GetAccommodationUnits;
using HMS.Modules.Hotels.Application.Queries.GetMealPlans;
using HMS.Modules.Hotels.Application.Queries.GetProperty;
using HMS.Modules.Hotels.Application.Queries.GetPropertyAvailability;
using HMS.Modules.Hotels.Application.Queries.GetPropertySettings;
using HMS.Modules.Hotels.Application.Queries.GetRatePlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Hotels.Api.Controllers;

[ApiController]
[Route("api/v1/properties")]
[Authorize]
public sealed class PropertiesController(ISender sender) : ControllerBase
{
    [HttpGet("{propertyUid:guid}")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertyQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/availability")]
    public async Task<IActionResult> GetAvailability(
        Guid propertyUid,
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int adults = 1,
        [FromQuery] int children = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetPropertyAvailabilityQuery(
            propertyUid,
            checkIn,
            checkOut,
            adults,
            children,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{propertyUid:guid}")]
    public async Task<IActionResult> Update(
        Guid propertyUid,
        [FromBody] UpdatePropertyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePropertyCommand(
            propertyUid,
            request.Name,
            request.PropertyType,
            request.Description,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.District,
            request.Province,
            request.PostalCode,
            request.CountryCode,
            request.Latitude,
            request.Longitude,
            request.Phone,
            request.Email,
            request.Timezone,
            request.DefaultCurrency,
            request.Status,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/settings")]
    public async Task<IActionResult> GetSettings(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPropertySettingsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{propertyUid:guid}/settings")]
    public async Task<IActionResult> UpdateSettings(
        Guid propertyUid,
        [FromBody] UpdatePropertySettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePropertySettingsCommand(
            propertyUid,
            request.CheckInTime,
            request.CheckOutTime,
            request.BookingNumberPrefix,
            request.InvoiceNumberPrefix,
            request.TaxRate,
            request.ServiceChargeRate,
            request.AllowOverbooking,
            request.ExtraSettings,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/accommodation-types")]
    public async Task<IActionResult> GetAccommodationTypes(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccommodationTypesQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{propertyUid:guid}/accommodation-types")]
    public async Task<IActionResult> CreateAccommodationType(
        Guid propertyUid,
        [FromBody] CreateAccommodationTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateAccommodationTypeCommand(
            propertyUid,
            request.Code,
            request.Name,
            request.UnitKind,
            request.Description,
            request.MaxAdults,
            request.MaxChildren,
            request.MaxOccupancy,
            request.DefaultQuantity,
            request.BaseRate,
            request.SortOrder,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/meal-plans")]
    public async Task<IActionResult> GetMealPlans(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMealPlansQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{propertyUid:guid}/meal-plans")]
    public async Task<IActionResult> CreateMealPlan(
        Guid propertyUid,
        [FromBody] CreateMealPlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateMealPlanCommand(
            propertyUid,
            request.Code,
            request.Name,
            request.Description,
            request.IncludesBreakfast,
            request.IncludesLunch,
            request.IncludesDinner,
            request.AllowByo,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/rate-plans")]
    public async Task<IActionResult> GetRatePlans(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRatePlansQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{propertyUid:guid}/rate-plans")]
    public async Task<IActionResult> CreateRatePlan(
        Guid propertyUid,
        [FromBody] CreateRatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRatePlanCommand(
            propertyUid,
            request.AccommodationTypeUid,
            request.MealPlanUid,
            request.Code,
            request.Name,
            request.PricingBasis,
            request.Currency,
            request.Description,
            request.IsRefundable,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("{propertyUid:guid}/units")]
    public async Task<IActionResult> GetUnits(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAccommodationUnitsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{propertyUid:guid}/units")]
    public async Task<IActionResult> CreateUnit(
        Guid propertyUid,
        [FromBody] CreateAccommodationUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateAccommodationUnitCommand(
            propertyUid,
            request.AccommodationTypeUid,
            request.UnitCode,
            request.UnitName,
            request.FloorOrArea,
            request.Status,
            request.HousekeepingStatus,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}

