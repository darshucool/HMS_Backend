using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Domain.Enums;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.CreateExpenseCategory;

public sealed record CreateExpenseCategoryCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    ExpenseGroup ExpenseGroup,
    Guid? ParentUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<ExpenseCategoryDto>>;

public sealed class CreateExpenseCategoryCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseCategoryRepository expenseCategoryRepository)
    : IRequestHandler<CreateExpenseCategoryCommand, FinanceResult<ExpenseCategoryDto>>
{
    public async Task<FinanceResult<ExpenseCategoryDto>> Handle(
        CreateExpenseCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, true, cancellationToken))
        {
            return FinanceResult<ExpenseCategoryDto>.Forbidden(
                "You cannot create expense categories for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<ExpenseCategoryDto>.NotFound("Property was not found.");

        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Trim().Length > 30)
            return FinanceResult<ExpenseCategoryDto>.Validation("Code is required and cannot exceed 30 characters.");

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
            return FinanceResult<ExpenseCategoryDto>.Validation("Name is required and cannot exceed 100 characters.");

        var code = request.Code.Trim().ToUpperInvariant();
        if (await expenseCategoryRepository.CodeExistsAsync(property.OrganizationId, code, cancellationToken))
            return FinanceResult<ExpenseCategoryDto>.Conflict("This expense category code already exists for the organization.");

        long? parentId = null;
        if (request.ParentUid is Guid parentUid)
        {
            parentId = await expenseCategoryRepository.GetIdAsync(parentUid, property.OrganizationId, cancellationToken);
            if (parentId is null)
                return FinanceResult<ExpenseCategoryDto>.NotFound("Parent category was not found for this organization.");
        }

        var uid = await expenseCategoryRepository.InsertAsync(
            property.OrganizationId,
            parentId,
            code,
            request.Name.Trim(),
            request.ExpenseGroup.ToDatabaseValue(),
            request.ActorSubject,
            cancellationToken);

        return FinanceResult<ExpenseCategoryDto>.Success(new ExpenseCategoryDto(
            uid,
            request.ParentUid,
            code,
            request.Name.Trim(),
            request.ExpenseGroup.ToDatabaseValue(),
            true));
    }
}
