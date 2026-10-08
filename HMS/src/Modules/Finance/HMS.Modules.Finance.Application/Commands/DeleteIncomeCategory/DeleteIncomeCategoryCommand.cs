using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.Queries.GetIncomeCategories;
using MediatR;

namespace HMS.Modules.Finance.Application.Commands.DeleteIncomeCategory;

public sealed record DeleteIncomeCategoryCommand(
    Guid PropertyUid,
    Guid IncomeCategoryUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<bool>>;

public sealed class DeleteIncomeCategoryCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IIncomeCategoryRepository incomeCategoryRepository)
    : IRequestHandler<DeleteIncomeCategoryCommand, FinanceResult<bool>>
{
    public async Task<FinanceResult<bool>> Handle(
        DeleteIncomeCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var property = await IncomeCategoryGuard.Load(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return FinanceResult<bool>.From(property.Error);

        var current = await incomeCategoryRepository.GetContextAsync(
            request.IncomeCategoryUid, property.OrganizationId, cancellationToken);
        if (current is null || current.IsArchived)
            return FinanceResult<bool>.NotFound("Income category was not found.");

        await incomeCategoryRepository.ArchiveAsync(current.Id, request.ActorSubject, cancellationToken);
        return FinanceResult<bool>.Success(true);
    }
}
