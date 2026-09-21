using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdateAccommodationUnit;

public sealed record UpdateAccommodationUnitCommand(
    Guid Uid,
    Guid AccommodationTypeUid,
    string UnitCode,
    string? UnitName,
    string? FloorOrArea,
    AccommodationUnitStatus Status,
    HousekeepingStatus HousekeepingStatus,
    string? Notes,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationUnitDto>>;

public sealed class UpdateAccommodationUnitCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository,
    IAccommodationUnitRepository accommodationUnitRepository)
    : IRequestHandler<UpdateAccommodationUnitCommand, HotelResult<AccommodationUnitDto>>
{
    public async Task<HotelResult<AccommodationUnitDto>> Handle(
        UpdateAccommodationUnitCommand request,
        CancellationToken cancellationToken)
    {
        var unit = await accommodationUnitRepository.GetByUidAsync(request.Uid, cancellationToken);
        if (unit is null || unit.IsArchived)
            return HotelResult<AccommodationUnitDto>.NotFound("Unit was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                unit.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<AccommodationUnitDto>.Forbidden("You cannot update this unit.");
        }

        if (string.IsNullOrWhiteSpace(request.UnitCode))
            return HotelResult<AccommodationUnitDto>.Validation("Unit code is required.");

        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.AccommodationTypeUid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<AccommodationUnitDto>.NotFound("Accommodation type was not found.");

        if (accommodationType.PropertyId != unit.PropertyId)
            return HotelResult<AccommodationUnitDto>.Validation(
                "The accommodation type does not belong to this property.");

        if (await accommodationUnitRepository.UnitCodeExistsAsync(
                unit.PropertyId,
                request.UnitCode.Trim(),
                unit.Uid,
                cancellationToken))
        {
            return HotelResult<AccommodationUnitDto>.Conflict(
                "This unit code already exists for the property.");
        }

        try
        {
            unit.Update(
                accommodationType.Id,
                accommodationType.Uid,
                request.UnitCode,
                request.UnitName,
                request.FloorOrArea,
                request.Status,
                request.HousekeepingStatus,
                request.Notes,
                request.IsActive,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<AccommodationUnitDto>.Validation(exception.Message);
        }

        await accommodationUnitRepository.UpdateAsync(unit, cancellationToken);
        return HotelResult<AccommodationUnitDto>.Success(PropertyMapper.ToAccommodationUnitDto(unit));
    }
}
