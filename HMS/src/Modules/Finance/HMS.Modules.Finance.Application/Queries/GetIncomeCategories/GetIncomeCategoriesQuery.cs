using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetIncomeCategories;

public sealed record GetIncomeCategoriesQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IReadOnlyList<IncomeCategoryDto>>>;

public sealed class GetIncomeCategoriesQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IIncomeCategoryRepository incomeCategoryRepository)
    : IRequestHandler<GetIncomeCategoriesQuery, FinanceResult<IReadOnlyList<IncomeCategoryDto>>>
{
    public async Task<FinanceResult<IReadOnlyList<IncomeCategoryDto>>> Handle(
        GetIncomeCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var property = await IncomeCategoryGuard.Load(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (property.Error is not null)
            return FinanceResult<IReadOnlyList<IncomeCategoryDto>>.From(property.Error);

        var items = await incomeCategoryRepository.ListAsync(property.OrganizationId, cancellationToken);
        return FinanceResult<IReadOnlyList<IncomeCategoryDto>>.Success(items);
    }
}

internal static class IncomeCategoryGuard
{
    public static async Task<(long OrganizationId, FinanceResult<object>? Error)> Load(
        IFinancePropertyAccess propertyAccess,
        Guid propertyUid,
        string actorSubject,
        bool isPlatformAdmin,
        bool requireManager,
        CancellationToken cancellationToken)
    {
        if (!isPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(actorSubject, propertyUid, requireManager, cancellationToken))
        {
            return (0, FinanceResult<object>.Forbidden(
                requireManager
                    ? "You cannot change income categories for this property."
                    : "You cannot view income categories for this property."));
        }

        var property = await propertyAccess.GetByUidAsync(propertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return (0, FinanceResult<object>.NotFound("Property was not found."));

        return (property.OrganizationId, null);
    }
}
