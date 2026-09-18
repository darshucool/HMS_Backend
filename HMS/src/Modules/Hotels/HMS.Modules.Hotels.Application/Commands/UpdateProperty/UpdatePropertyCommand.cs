using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdateProperty;

public sealed record UpdatePropertyCommand(
    Guid PropertyUid,
    string Name,
    PropertyType PropertyType,
    string? Description,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? District,
    string? Province,
    string? PostalCode,
    string CountryCode,
    decimal? Latitude,
    decimal? Longitude,
    string? Phone,
    string? Email,
    string Timezone,
    string DefaultCurrency,
    PropertyStatus Status,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<PropertyDto>>;

public sealed class UpdatePropertyCommandHandler(IPropertyRepository repository)
    : IRequestHandler<UpdatePropertyCommand, HotelResult<PropertyDto>>
{
    public async Task<HotelResult<PropertyDto>> Handle(
        UpdatePropertyCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await repository.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return HotelResult<PropertyDto>.Forbidden("You cannot update this property.");
        }

        var property = await repository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<PropertyDto>.NotFound("Property was not found.");

        try
        {
            property.Update(
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
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<PropertyDto>.Validation(exception.Message);
        }

        await repository.UpdateAsync(property, cancellationToken);
        return HotelResult<PropertyDto>.Success(PropertyMapper.ToDto(property));
    }
}

