using HMS.Modules.Finance.Application.DTOs;
using UtilityBillEntity = HMS.Modules.Finance.Domain.Entities.UtilityBill;

namespace HMS.Modules.Finance.Application.Abstractions;

public sealed record UtilityTypeRef(
    long Id,
    Guid Uid,
    string Name,
    string? UnitOfMeasure,
    long OrganizationId);

public interface IUtilityBillRepository
{
    Task<UtilityTypeRef?> GetTypeAsync(Guid utilityTypeUid, CancellationToken cancellationToken);
    Task InsertAsync(UtilityBillEntity bill, CancellationToken cancellationToken);
    Task<IReadOnlyList<UtilityBillDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
    Task<UtilityBillEntity?> GetByUidAsync(Guid utilityBillUid, CancellationToken cancellationToken);
    Task<UtilityBillDto?> GetDetailByUidAsync(Guid utilityBillUid, CancellationToken cancellationToken);
    Task UpdateAsync(UtilityBillEntity bill, CancellationToken cancellationToken);
}
