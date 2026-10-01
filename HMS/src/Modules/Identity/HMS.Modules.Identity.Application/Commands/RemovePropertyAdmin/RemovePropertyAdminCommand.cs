using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Application.DTOs;
using MediatR;

namespace HMS.Modules.Identity.Application.Commands.RemovePropertyAdmin;

public sealed record RemovePropertyAdminCommand(Guid PropertyUid, Guid AdminId)
    : IRequest<PropertyAdminResult<bool>>;

internal sealed class RemovePropertyAdminCommandHandler(IStaffRepository staffRepository)
    : IRequestHandler<RemovePropertyAdminCommand, PropertyAdminResult<bool>>
{
    public async Task<PropertyAdminResult<bool>> Handle(
        RemovePropertyAdminCommand request,
        CancellationToken cancellationToken)
    {
        var property = await staffRepository.GetPropertyByUidAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return PropertyAdminResult<bool>.Failure("not_found", "Property was not found.");

        var removed = await staffRepository.RemovePropertyAdminAsync(
            request.PropertyUid,
            request.AdminId,
            cancellationToken);

        return removed
            ? PropertyAdminResult<bool>.Success(true)
            : PropertyAdminResult<bool>.Failure("not_found", "Admin was not found for this property.");
    }
}
