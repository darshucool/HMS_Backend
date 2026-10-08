using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategories;
using MediatR;

namespace HMS.Modules.Finance.Application.Queries.GetIncomeCategory;

public sealed record GetIncomeCategoryQuery(
    Guid PropertyUid,
    Guid IncomeCategoryUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<IncomeCategoryDto>>;

public sealed class GetIncomeCategoryQueryHandler(
    IFinancePropertyAccess propertyAccess,
    IIncomeCategoryRepository incomeCategoryRepository)
    : IRequestHandler<GetIncomeCategoryQuery, FinanceResult<IncomeCategoryDto>>
{
    public async Task<FinanceResult<IncomeCategoryDto>> Handle(
        GetIncomeCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var property = await IncomeCategoryGuard.Load(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, false, cancellationToken);
        if (property.Error is not null)
            return FinanceResult<IncomeCategoryDto>.From(property.Error);

        var item = await incomeCategoryRepository.GetDetailAsync(
            request.IncomeCategoryUid, property.OrganizationId, cancellationToken);

        return item is null
            ? FinanceResult<IncomeCategoryDto>.NotFound("Income category was not found.")
            : FinanceResult<IncomeCategoryDto>.Success(item);
    }
}
