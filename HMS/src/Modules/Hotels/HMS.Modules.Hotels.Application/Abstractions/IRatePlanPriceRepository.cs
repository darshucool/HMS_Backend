using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IRatePlanPriceRepository
{
    Task InsertAsync(RatePlanPrice price, CancellationToken cancellationToken);
}
