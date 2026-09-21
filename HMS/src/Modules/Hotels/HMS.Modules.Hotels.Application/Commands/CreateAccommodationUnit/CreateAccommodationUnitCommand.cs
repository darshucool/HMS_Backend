using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateAccommodationUnit;

public sealed record CreateAccommodationUnitCommand(
    Guid PropertyUid,
    Guid AccommodationTypeUid,
    string UnitCode,
    string? UnitName,
    string? FloorOrArea,
    AccommodationUnitStatus Status,
    HousekeepingStatus HousekeepingStatus,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationUnitDto>>;

public sealed class CreateAccommodationUnitCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository,
    IAccommodationUnitRepository accommodationUnitRepository)
    : IRequestHandler<CreateAccommodationUnitCommand, HotelResult<AccommodationUnitDto>>
{
    public async Task<HotelResult<AccommodationUnitDto>> Handle(
        CreateAccommodationUnitCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<AccommodationUnitDto>.Forbidden(
                "You cannot create units for this property.");
        }

        if (string.IsNullOrWhiteSpace(request.UnitCode))
            return HotelResult<AccommodationUnitDto>.Validation("Unit code is required.");

        var property = await propertyRepository.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return HotelResult<AccommodationUnitDto>.NotFound("Property was not found.");

        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.AccommodationTypeUid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<AccommodationUnitDto>.NotFound("Accommodation type was not found.");

        if (accommodationType.PropertyId != property.Id)
            return HotelResult<AccommodationUnitDto>.Validation(
                "The accommodation type does not belong to this property.");

        if (await accommodationUnitRepository.UnitCodeExistsAsync(
                property.Id,
                request.UnitCode.Trim(),
                excludeUid: null,
                cancellationToken))
        {
            return HotelResult<AccommodationUnitDto>.Conflict(
                "This unit code already exists for the property.");
        }

        AccommodationUnit unit;
        try
        {
            unit = AccommodationUnit.Create(
                property.OrganizationId,
                property.Id,
                property.Uid,
                accommodationType.Id,
                accommodationType.Uid,
                request.UnitCode,
                request.UnitName,
                request.FloorOrArea,
                request.Status,
                request.HousekeepingStatus,
                request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<AccommodationUnitDto>.Validation(exception.Message);
        }

        await accommodationUnitRepository.InsertAsync(unit, cancellationToken);
        return HotelResult<AccommodationUnitDto>.Success(PropertyMapper.ToAccommodationUnitDto(unit));
    }
}
