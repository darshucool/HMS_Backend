using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using MediatR;

namespace HMS.Modules.Staff.Application.Queries.GetStaffPayments;

public sealed record GetStaffPaymentsQuery(
    Guid PropertyUid,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<IReadOnlyList<StaffPaymentDto>>>;

public sealed class GetStaffPaymentsQueryHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<GetStaffPaymentsQuery, StaffResult<IReadOnlyList<StaffPaymentDto>>>
{
    public async Task<StaffResult<IReadOnlyList<StaffPaymentDto>>> Handle(
        GetStaffPaymentsQuery request,
        CancellationToken cancellationToken)
    {
        var property = await propertyAccess.GetPropertyAsync(request.PropertyUid, cancellationToken);
        if (property is null || property.IsArchived)
            return StaffResult<IReadOnlyList<StaffPaymentDto>>.NotFound("Property was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, request.PropertyUid, false, cancellationToken))
        {
            return StaffResult<IReadOnlyList<StaffPaymentDto>>.Forbidden(
                "You cannot view staff payments for this property.");
        }

        var items = await staffRepository.GetPaymentsByPropertyAsync(property.Id, cancellationToken);
        return StaffResult<IReadOnlyList<StaffPaymentDto>>.Success(items);
    }
}
