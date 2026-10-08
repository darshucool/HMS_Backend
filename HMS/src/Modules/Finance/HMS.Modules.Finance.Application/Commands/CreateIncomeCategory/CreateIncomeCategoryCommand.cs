using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategories;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.CreateIncomeCategory;

public sealed record CreateIncomeCategoryCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IncomeCategoryDto>>;

public sealed class CreateIncomeCategoryCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IIncomeCategoryRepository incomeCategoryRepository)
    : IRequestHandler<CreateIncomeCategoryCommand, FinanceResult<IncomeCategoryDto>>
{
    public async Task<FinanceResult<IncomeCategoryDto>> Handle(
        CreateIncomeCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var property = await IncomeCategoryGuard.Load(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return FinanceResult<IncomeCategoryDto>.From(property.Error);

        var validation = IncomeCategoryRules.Validate(request.Code, request.Name);
        if (validation is not null)
            return FinanceResult<IncomeCategoryDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();
        if (await incomeCategoryRepository.CodeExistsAsync(property.OrganizationId, code, null, cancellationToken))
            return FinanceResult<IncomeCategoryDto>.Conflict("This income category code already exists for the organization.");

        var uid = await incomeCategoryRepository.InsertAsync(
            property.OrganizationId, code, name, request.IsActive, request.ActorSubject, cancellationToken);

        return FinanceResult<IncomeCategoryDto>.Success(new IncomeCategoryDto(uid, code, name, request.IsActive));
    }
}

internal static class IncomeCategoryRules
{
    public static string? Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
            return "Code is required and cannot exceed 30 characters.";

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            return "Name is required and cannot exceed 100 characters.";

        return null;
    }
}
