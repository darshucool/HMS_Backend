namespace HMS.Modules.Guests.Application.Abstractions;

public sealed record PropertyAccessContext(
    long Id,
    Guid Uid,
    long OrganizationId,
    Guid OrganizationUid,
    bool IsArchived);

public interface IPropertyAccess
{
    Task<PropertyAccessContext?> GetByUidAsync(Guid propertyUid, CancellationToken cancellationToken);
    Task<bool> HasAccessAsync(
        string actorSubject,
        Guid propertyUid,
        bool requireManager,
        CancellationToken cancellationToken);
    Task<bool> HasAccessToOrganizationAsync(
        string actorSubject,
        long organizationId,
        bool requireManager,
        CancellationToken cancellationToken);
}
