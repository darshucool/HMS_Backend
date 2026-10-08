using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Commands.CreateIncomeCategory;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategories;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.UpdateIncomeCategory;

public sealed record UpdateIncomeCategoryCommand(
    Guid PropertyUid,
    Guid IncomeCategoryUid,
    string Code,
    string Name,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IncomeCategoryDto>>;

public sealed class UpdateIncomeCategoryCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IIncomeCategoryRepository incomeCategoryRepository)
    : IRequestHandler<UpdateIncomeCategoryCommand, FinanceResult<IncomeCategoryDto>>
{
    public async Task<FinanceResult<IncomeCategoryDto>> Handle(
        UpdateIncomeCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var property = await IncomeCategoryGuard.Load(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return FinanceResult<IncomeCategoryDto>.From(property.Error);

        var current = await incomeCategoryRepository.GetContextAsync(
            request.IncomeCategoryUid, property.OrganizationId, cancellationToken);
        if (current is null || current.IsArchived)
            return FinanceResult<IncomeCategoryDto>.NotFound("Income category was not found.");

        var validation = IncomeCategoryRules.Validate(request.Code, request.Name);
        if (validation is not null)
            return FinanceResult<IncomeCategoryDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        if (await incomeCategoryRepository.CodeExistsAsync(property.OrganizationId, code, current.Uid, cancellationToken))
            return FinanceResult<IncomeCategoryDto>.Conflict("This income category code already exists for the organization.");

        await incomeCategoryRepository.UpdateAsync(
            current.Id, code, name, request.IsActive, request.ActorSubject, cancellationToken);

        return FinanceResult<IncomeCategoryDto>.Success(
            new IncomeCategoryDto(current.Uid, code, name, request.IsActive));
    }
}
