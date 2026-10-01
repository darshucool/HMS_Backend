using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.Commands.CreateUtilityType;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetUtilityTypes;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.UpdateUtilityType;

public sealed record UpdateUtilityTypeCommand(
    Guid PropertyUid,
    Guid UtilityTypeUid,
    string Code,
    string Name,
    string? UnitOfMeasure,
    bool IsMetered,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityTypeDto>>;

public sealed class UpdateUtilityTypeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityTypeRepository utilityTypeRepository)
    : IRequestHandler<UpdateUtilityTypeCommand, FinanceResult<UtilityTypeDto>>
{
    public async Task<FinanceResult<UtilityTypeDto>> Handle(
        UpdateUtilityTypeCommand request,
        CancellationToken cancellationToken)
    {
        var access = await GetUtilityTypesQueryHandler.Authorize<UtilityTypeDto>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (access.Error is not null)
            return access.Error;

        var existing = await utilityTypeRepository.GetByUidAsync(
            request.UtilityTypeUid, access.OrganizationId, cancellationToken);
        if (existing is null || existing.IsArchived)
            return FinanceResult<UtilityTypeDto>.NotFound("Utility type was not found.");

        var validation = CreateUtilityTypeCommandHandler.Validate(request.Code, request.Name, request.UnitOfMeasure);
        if (validation is not null)
            return FinanceResult<UtilityTypeDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await utilityTypeRepository.CodeExistsAsync(access.OrganizationId, code, existing.Uid, cancellationToken))
            return FinanceResult<UtilityTypeDto>.Conflict("This utility type code already exists for the organization.");

        var name = request.Name.Trim();
        var unit = CreateUtilityTypeCommandHandler.TrimOrNull(request.UnitOfMeasure);
        await utilityTypeRepository.UpdateAsync(
            existing.Id, code, name, unit, request.IsMetered, request.ActorSubject, cancellationToken);

        var detail = await utilityTypeRepository.GetDetailAsync(
            request.UtilityTypeUid, access.OrganizationId, cancellationToken);
        return detail is null
            ? FinanceResult<UtilityTypeDto>.NotFound("Utility type was not found.")
            : FinanceResult<UtilityTypeDto>.Success(detail);
    }
}
