using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.DeleteAccommodationType;

public sealed record DeleteAccommodationTypeCommand(
    Guid Uid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<bool>>;

public sealed class DeleteAccommodationTypeCommandHandler(
    IPropertyRepository propertyRepository,
    IAccommodationTypeRepository accommodationTypeRepository)
    : IRequestHandler<DeleteAccommodationTypeCommand, HotelResult<bool>>
{
    public async Task<HotelResult<bool>> Handle(
        DeleteAccommodationTypeCommand request,
        CancellationToken cancellationToken)
    {
        var accommodationType = await accommodationTypeRepository.GetByUidAsync(
            request.Uid,
            cancellationToken);

        if (accommodationType is null || accommodationType.IsArchived)
            return HotelResult<bool>.NotFound("Accommodation type was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                accommodationType.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<bool>.Forbidden("You cannot delete this accommodation type.");
        }

        accommodationType.Archive(request.ActorSubject);
        await accommodationTypeRepository.ArchiveAsync(accommodationType, cancellationToken);
        return HotelResult<bool>.Success(true);
    }
}
