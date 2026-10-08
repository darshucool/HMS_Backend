using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetExpenseCategories;

public sealed record GetExpenseCategoriesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<ExpenseCategoryDto>>>;

public sealed class GetExpenseCategoriesQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IExpenseCategoryRepository expenseCategoryRepository)
    : IRequestHandler<GetExpenseCategoriesQuery, FinanceResult<IReadOnlyList<ExpenseCategoryDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<ExpenseCategoryDto>>> Handle(
        GetExpenseCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                false,
                cancellationToken))
        {
            return FinanceResult<IReadOnlyList<ExpenseCategoryDto>>.Forbidden(
                "You cannot view expense categories for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<IReadOnlyList<ExpenseCategoryDto>>.NotFound("Property was not found.");

        var items = await expenseCategoryRepository.GetByOrganizationIdAsync(
            property.OrganizationId,
            cancellationToken);

        return FinanceResult<IReadOnlyList<ExpenseCategoryDto>>.Success(items);
    }
}
