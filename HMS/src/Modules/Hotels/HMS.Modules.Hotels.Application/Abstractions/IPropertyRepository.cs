using HMS.Modules.Hotels.Application.DTOs;
using HMS.Modules.Hotels.Domain.Entities;

namespace HMS.Modules.Hotels.Application.Abstractions;

public interface IPropertyRepository
{
    Task<Property?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<PropertySettings?> GetSettingsAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(long organizationId, string code, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
    Task InsertAsync(Property property, PropertySettings defaultSettings, CancellationToken cancellationToken);
    Task UpdateAsync(Property property, CancellationToken cancellationToken);
    Task UpdateSettingsAsync(PropertySettings settings, CancellationToken cancellationToken);
    Task<IReadOnlyList<MyPropertyDto>> GetMyPropertiesAsync(
        string actorSubject,
        CancellationToken cancellationToken);
}

