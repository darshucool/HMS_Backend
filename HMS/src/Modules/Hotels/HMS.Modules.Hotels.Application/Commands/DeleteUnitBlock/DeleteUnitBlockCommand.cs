using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Application.Common;
using MediatR;

namespace HMS.Modules.Hotels.Application.Commands.DeleteUnitBlock;

public sealed record DeleteUnitBlockCommand(
    Guid Uid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<HotelResult<bool>>;

public sealed class DeleteUnitBlockCommandHandler(
    IPropertyRepository propertyRepository,
    IUnitBlockRepository unitBlockRepository)
    : IRequestHandler<DeleteUnitBlockCommand, HotelResult<bool>>
{
    public async Task<HotelResult<bool>> Handle(
        DeleteUnitBlockCommand request,
        CancellationToken cancellationToken)
    {
        var block = await unitBlockRepository.GetByUidAsync(request.Uid, cancellationToken);
        if (block is null || block.IsArchived)
            return HotelResult<bool>.NotFound("Unit block was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyRepository.HasAccessAsync(
                request.ActorSubject,
                block.PropertyUid,
                true,
                cancellationToken))
        {
            return HotelResult<bool>.Forbidden("You cannot delete this unit block.");
        }

        block.Archive(request.ActorSubject);
        await unitBlockRepository.ArchiveAsync(block, cancellationToken);
        return HotelResult<bool>.Success(true);
    }
}
