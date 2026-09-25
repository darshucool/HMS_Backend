using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;
using HMS.Modules.Hotels.Domain.Enums;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.CreateUnitBlock;

public sealed record CreateUnitBlockCommand(
    Guid UnitUid,
    DateOnly StartDate,
    DateOnly EndDate,
    UnitBlockType BlockType,
    string? Reason,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<UnitBlockDto>>;

public sealed class CreateUnitBlockCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationUnitRepository accommodationUnitRepository,
    IUnitBlockRepository unitBlockRepository)
    : IRequestHandler<CreateUnitBlockCommand, HotelResult<UnitBlockDto>>
{
    public async Task<HotelResult<UnitBlockDto>> Handle(
        CreateUnitBlockCommand request,
        CancellationToken cancellationToken)
    {
        var unit = await accommodationUnitRepository.GetByUidAsync(request.UnitUid, cancellationToken);
        if (unit is null || unit.IsArchived)
            return HotelResult<UnitBlockDto>.NotFound("Unit was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                unit.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<UnitBlockDto>.Forbidden("You cannot create blocks for this unit.");
        }

        if (await unitBlockRepository.OverlapsAsync(
                unit.Id,
                request.StartDate,
                request.EndDate,
                cancellationToken))
        {
            return HotelResult<UnitBlockDto>.Conflict(
                "This unit already has an overlapping block for the selected dates.");
        }

        UnitBlock block;
        try
        {
            block = UnitBlock.Create(
                unit.OrganizationId,
                unit.PropertyId,
                unit.PropertyUid,
                unit.Id,
                unit.Uid,
                request.StartDate,
                request.EndDate,
                request.BlockType,
                request.Reason,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return HotelResult<UnitBlockDto>.Validation(exception.Message);
        }

        await unitBlockRepository.InsertAsync(block, cancellationToken);
        return HotelResult<UnitBlockDto>.Success(PropertyMapper.ToUnitBlockDto(block));
    }
}
