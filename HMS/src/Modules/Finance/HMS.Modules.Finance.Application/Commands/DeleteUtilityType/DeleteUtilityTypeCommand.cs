using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.Queries.GetUtilityTypes;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.DeleteUtilityType;

public sealed record DeleteUtilityTypeCommand(
    Guid PropertyUid,
    Guid UtilityTypeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<bool>>;

public sealed class DeleteUtilityTypeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityTypeRepository utilityTypeRepository)
    : IRequestHandler<DeleteUtilityTypeCommand, FinanceResult<bool>>
{
    public async Task<FinanceResult<bool>> Handle(
        DeleteUtilityTypeCommand request,
        CancellationToken cancellationToken)
    {
        var access = await GetUtilityTypesQueryHandler.Authorize<bool>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (access.Error is not null)
            return access.Error;

        var existing = await utilityTypeRepository.GetByUidAsync(
            request.UtilityTypeUid, access.OrganizationId, cancellationToken);
        if (existing is null || existing.IsArchived)
            return FinanceResult<bool>.NotFound("Utility type was not found.");

        await utilityTypeRepository.ArchiveAsync(existing.Id, request.ActorSubject, cancellationToken);
        return FinanceResult<bool>.Success(true);
    }
}
