using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IMealPlanRepository
{
    Task<MealPlan?> GetByUidAsync(Guid uid, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(
        long propertyId,
        string code,
        CancellationToken cancellationToken);

    Task InsertAsync(MealPlan mealPlan, CancellationToken cancellationToken);

    Task<IReadOnlyList<MealPlanDto>> GetByPropertyUidAsync(
        Guid propertyUid,
        CancellationToken cancellationToken);
}
