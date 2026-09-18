using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateAccommodationType;

public sealed record CreateAccommodationTypeCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    UnitKind UnitKind,
    string? Description,
    int MaxAdults,
    int MaxChildren,
    int MaxOccupancy,
    int DefaultQuantity,
    decimal BaseRate,
    int SortOrder,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationTypeDto>>;

public sealed class CreateAccommodationTypeCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository)
    : IRequestHandler<CreateAccommodationTypeCommand, HotelResult<AccommodationTypeDto>>
{
    public async Task<HotelResult<AccommodationTypeDto>> Handle(
        CreateAccommodationTypeCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<AccommodationTypeDto>.Forbidden(
                "You cannot create accommodation types for this property.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<AccommodationTypeDto>.Validation("Code and name are required.");

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<AccommodationTypeDto>.NotFound("Property was not found.");

        if (await accommodationTypeRepository.CodeExistsAsync(
                property.Id,
                request.Code.Trim().ToUpperInvariant(),
                cancellationToken))
        {
            return HotelResult<AccommodationTypeDto>.Conflict(
                "This accommodation type code already exists for the property.");
        }

        AccommodationType accommodationType;
        try
        {
            accommodationType = AccommodationType.Create(
                property.OrganizationId,
                property.Id,
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
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<AccommodationTypeDto>.Validation(exception.Message);
        }

        await accommodationTypeRepository.InsertAsync(accommodationType, cancellationToken);
        return HotelResult<AccommodationTypeDto>.Success(
            PropertyMapper.ToAccommodationTypeDto(property.Uid, accommodationType));
    }
}
