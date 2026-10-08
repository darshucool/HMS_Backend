using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using MediatR;
using OtherIncomeEntity = HMS.Modules.Finance.Domain.Entities.OtherIncome;

namespace HMS.Modules.Finance.Application.Commands.DeleteOtherIncome;

public sealed record DeleteOtherIncomeCommand(
    Guid PropertyUid,
    Guid OtherIncomeUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<bool>>;

public sealed class DeleteOtherIncomeCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IOtherIncomeRepository otherIncomeRepository)
    : IRequestHandler<DeleteOtherIncomeCommand, FinanceResult<bool>>
{
    public async Task<FinanceResult<bool>> Handle(
        DeleteOtherIncomeCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return FinanceResult<bool>.Forbidden(
                "You cannot delete other income for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<bool>.NotFound("Property was not found.");

        var current = await otherIncomeRepository.GetContextAsync(
            request.PropertyUid,
            request.OtherIncomeUid,
            cancellationToken);
        if (current is null || current.IsArchived)
            return FinanceResult<bool>.NotFound("Other income was not found.");

        var income = OtherIncomeEntity.Rehydrate(
            current.Id,
            current.Uid,
            current.OrganizationId,
            current.PropertyId,
            current.PropertyUid,
            current.IsActive,
            current.IsArchived,
            current.CreationDate,
            current.CreatedBy);

        income.Archive(request.ActorSubject);
        await otherIncomeRepository.ArchiveAsync(income, cancellationToken);
        return FinanceResult<bool>.Success(true);
    }
}
