using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.UpdateAccommodationType;

public sealed record UpdateAccommodationTypeCommand(
    Guid Uid,
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
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<AccommodationTypeDto>>;

public sealed class UpdateAccommodationTypeCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository)
    : IRequestHandler<UpdateAccommodationTypeCommand, HotelResult<AccommodationTypeDto>>
{
    public async Task<HotelResult<AccommodationTypeDto>> Handle(
        UpdateAccommodationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.Uid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<AccommodationTypeDto>.NotFound("Accommodation type was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                accommodationType.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<AccommodationTypeDto>.Forbidden(
                "You cannot update this accommodation type.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return HotelResult<AccommodationTypeDto>.Validation("Code and name are required.");

        if (await accommodationTypeRepository.CodeExistsAsync(
                accommodationType.PropertyId,
                request.Code.Trim().ToUpperInvariant(),
                accommodationType.Uid,
                cancellationToken))
        {
            return HotelResult<AccommodationTypeDto>.Conflict(
                "This accommodation type code already exists for the property.");
        }

        try
        {
            accommodationType.Update(
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
                request.IsActive,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<AccommodationTypeDto>.Validation(exception.Message);
        }

        await accommodationTypeRepository.UpdateAsync(accommodationType, cancellationToken);
        return HotelResult<AccommodationTypeDto>.Success(
            PropertyMapper.ToAccommodationTypeDto(accommodationType));
    }
}
