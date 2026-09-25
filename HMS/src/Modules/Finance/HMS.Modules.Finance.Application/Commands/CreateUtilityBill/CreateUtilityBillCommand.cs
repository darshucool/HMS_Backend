using HMS.Modules.Finance.Application.Abstractions;
using HMS.Modules.Finance.Application.Common;
using HMS.Modules.Finance.Application.DTOs;
using MediatR;
using UtilityBillEntity = HMS.Modules.Finance.Domain.Entities.UtilityBill;

namespace HMS.Modules.Finance.Application.Commands.CreateUtilityBill;

public sealed record CreateUtilityBillCommand(
    Guid PropertyUid,
    Guid UtilityTypeUid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal? PreviousReading,
    decimal? CurrentReading,
    decimal? UnitsUsed,
    decimal Amount,
    string Currency,
    DateOnly? DueDate,
    DateTimeOffset? PaidAt,
    string? ReferenceNumber,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<FinanceResult<UtilityBillDto>>;

public sealed class CreateUtilityBillCommandHandler(
    IFinancePropertyAccess propertyAccess,
    IUtilityBillRepository utilityBillRepository)
    : IRequestHandler<CreateUtilityBillCommand, FinanceResult<UtilityBillDto>>
{
    public async Task<FinanceResult<UtilityBillDto>> Handle(
        CreateUtilityBillCommand request,
        CancellationToken cancellationToken)
    {
        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(
                request.ActorSubject,
                request.PropertyUid,
                true,
                cancellationToken))
        {
            return FinanceResult<UtilityBillDto>.Forbidden(
                "You cannot create utility bills for this property.");
        }

        var property = await propertyAccess.GetByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return FinanceResult<UtilityBillDto>.NotFound("Property was not found.");

        var utilityType = await utilityBillRepository.GetTypeAsync(request.UtilityTypeUid, cancellationToken);
        if (utilityType is null || utilityType.OrganizationId != property.OrganizationId)
            return FinanceResult<UtilityBillDto>.NotFound("Utility type was not found.");

        UtilityBillEntity bill;
        try
        {
            bill = UtilityBillEntity.Create(
                property.OrganizationId,
                property.Id,
                request.PropertyUid,
                utilityType.Id,
                utilityType.Uid,
                request.PeriodStart,
                request.PeriodEnd,
                request.PreviousReading,
                request.CurrentReading,
                request.UnitsUsed,
                request.Amount,
                request.Currency,
                request.DueDate,
                request.PaidAt,
                request.ReferenceNumber,
                request.Notes,
                request.ActorSubject);
        }
        catch (ArgumentException exception)
        {
            return FinanceResult<UtilityBillDto>.Validation(exception.Message);
        }

        await utilityBillRepository.InsertAsync(bill, cancellationToken);

        return FinanceResult<UtilityBillDto>.Success(new UtilityBillDto(
            bill.Uid,
            bill.PropertyUid,
            bill.UtilityTypeUid,
            utilityType.Name,
            utilityType.UnitOfMeasure,
            bill.PeriodStart,
            bill.PeriodEnd,
            bill.PreviousReading,
            bill.CurrentReading,
            bill.UnitsUsed,
            bill.Amount,
            bill.Currency,
            bill.DueDate,
            bill.PaidAt,
            bill.ReferenceNumber,
            bill.Notes,
            bill.CreationDate));
    }
}
