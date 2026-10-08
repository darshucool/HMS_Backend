using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetUtilityTypes;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.CreateUtilityType;

public sealed record CreateUtilityTypeCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    string? UnitOfMeasure,
    bool IsMetered,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityTypeDto>>;

public sealed class CreateUtilityTypeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityTypeRepository utilityTypeRepository)
    : IRequestHandler<CreateUtilityTypeCommand, FinanceResult<UtilityTypeDto>>
{
    public async Task<FinanceResult<UtilityTypeDto>> Handle(
        CreateUtilityTypeCommand request,
        CancellationToken cancellationToken)
    {
        var access = await GetUtilityTypesQueryHandler.Authorize<UtilityTypeDto>(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (access.Error is not null)
            return access.Error;

        var validation = Validate(request.Code, request.Name, request.UnitOfMeasure);
        if (validation is not null)
            return FinanceResult<UtilityTypeDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await utilityTypeRepository.CodeExistsAsync(access.OrganizationId, code, null, cancellationToken))
            return FinanceResult<UtilityTypeDto>.Conflict("This utility type code already exists for the organization.");

        var name = request.Name.Trim();
        var unit = TrimOrNull(request.UnitOfMeasure);
        var uid = await utilityTypeRepository.InsertAsync(
            access.OrganizationId, code, name, unit, request.IsMetered, request.ActorSubject, cancellationToken);

        return FinanceResult<UtilityTypeDto>.Success(new UtilityTypeDto(uid, code, name, unit, request.IsMetered, true));
    }

    internal static string? Validate(string code, string name, string? unitOfMeasure)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
            return "Code is required and cannot exceed 30 characters.";

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            return "Name is required and cannot exceed 100 characters.";

        if (unitOfMeasure is { Length: > 30 })
            return "Unit of measure cannot exceed 30 characters.";

        return null;
    }

    internal static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
