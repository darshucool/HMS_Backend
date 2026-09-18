using System.Text.Json;
using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdatePropertySettings;

public sealed record UpdatePropertySettingsCommand(
    Guid PropertyUid,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    string BookingNumberPrefix,
    string InvoiceNumberPrefix,
    decimal TaxRate,
    decimal ServiceChargeRate,
    bool AllowOverbooking,
    JsonElement? ExtraSettings,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<PropertySettingsDto>>;

public sealed class UpdatePropertySettingsCommandHandler(IPropertyRepository repository)
    : IRequestHandler<UpdatePropertySettingsCommand, HotelResult<PropertySettingsDto>>
{
    public async Task<HotelResult<PropertySettingsDto>> Handle(
        UpdatePropertySettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await repository.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return HotelResult<PropertySettingsDto>.Forbidden("You cannot update settings for this property.");
        }

        var settings = await repository.GetSettingsAsync(request.PropertyUid, cancellationToken);
        if (settings is null)
            return HotelResult<PropertySettingsDto>.NotFound("Property settings were not found.");

        try
        {
            settings.Update(
                request.CheckInTime,
                request.CheckOutTime,
                request.BookingNumberPrefix,
                request.InvoiceNumberPrefix,
                request.TaxRate,
                request.ServiceChargeRate,
                request.AllowOverbooking,
                request.ExtraSettings?.GetRawText() ?? "{}",
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<PropertySettingsDto>.Validation(exception.Message);
        }

        await repository.UpdateSettingsAsync(settings, cancellationToken);
        return HotelResult<PropertySettingsDto>.Success(
            PropertyMapper.ToSettingsDto(request.PropertyUid, settings));
    }
}

