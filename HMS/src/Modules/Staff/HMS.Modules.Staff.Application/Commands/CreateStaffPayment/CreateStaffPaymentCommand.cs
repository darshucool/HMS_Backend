using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Application.Common;
using HMS.Modules.Staff.Application.DTOs;
using HMS.Modules.Staff.Domain.Enums;
using MediatR;

namespace HMS.Modules.Staff.Application.Commands.CreateStaffPayment;

public sealed record CreateStaffPaymentCommand(
    Guid StaffUid,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal BasicAmount,
    decimal OvertimeAmount,
    decimal BonusAmount,
    decimal DeductionAmount,
    StaffPaymentMethod PaymentMethod,
    DateTimeOffset? PaidAt,
    string? ReferenceNumber,
    string? Notes,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<StaffResult<StaffPaymentDto>>;

public sealed class CreateStaffPaymentCommandHandler(
    IStaffPropertyAccess propertyAccess,
    IStaffRepository staffRepository)
    : IRequestHandler<CreateStaffPaymentCommand, StaffResult<StaffPaymentDto>>
{
    public async Task<StaffResult<StaffPaymentDto>> Handle(
        CreateStaffPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetContextAsync(request.StaffUid, cancellationToken);
        if (staff is null || staff.IsArchived)
            return StaffResult<StaffPaymentDto>.NotFound("Staff member was not found.");

        if (!request.IsPlatformAdmin &&
            !await propertyAccess.HasAccessAsync(request.ActorSubject, staff.PropertyUid, true, cancellationToken))
        {
            return StaffResult<StaffPaymentDto>.Forbidden("You cannot add payments for this staff member.");
        }

        if (request.PeriodEnd < request.PeriodStart)
            return StaffResult<StaffPaymentDto>.Validation("Period end cannot be before period start.");

        if (request.BasicAmount < 0 || request.OvertimeAmount < 0 ||
            request.BonusAmount < 0 || request.DeductionAmount < 0)
        {
            return StaffResult<StaffPaymentDto>.Validation("Payment amounts cannot be negative.");
        }

        var totalPaid = request.BasicAmount + request.OvertimeAmount + request.BonusAmount - request.DeductionAmount;
        if (totalPaid < 0)
            return StaffResult<StaffPaymentDto>.Validation("Deductions cannot exceed the payment amounts.");

        if (request.ReferenceNumber is { Length: > 150 })
            return StaffResult<StaffPaymentDto>.Validation("Reference number cannot exceed 150 characters.");

        var uid = await staffRepository.InsertPaymentAsync(
            staff.OrganizationId,
            staff.PropertyId,
            staff.Id,
            request.PeriodStart,
            request.PeriodEnd,
            request.BasicAmount,
            request.OvertimeAmount,
            request.BonusAmount,
            request.DeductionAmount,
            totalPaid,
            request.PaymentMethod.ToDatabaseValue(),
            request.PaidAt ?? DateTimeOffset.UtcNow,
            StaffProfileRules.TrimOrNull(request.ReferenceNumber),
            StaffProfileRules.TrimOrNull(request.Notes),
            request.ActorSubject,
            cancellationToken);

        var detail = await staffRepository.GetPaymentAsync(uid, cancellationToken);
        return detail is null
            ? StaffResult<StaffPaymentDto>.NotFound("Staff payment was not found.")
            : StaffResult<StaffPaymentDto>.Success(detail);
    }
}
