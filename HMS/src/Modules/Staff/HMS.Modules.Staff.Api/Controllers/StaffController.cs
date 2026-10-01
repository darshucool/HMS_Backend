using HMS.Modules.Staff.Api.Contracts;
using HMS.Modules.Staff.Application.Commands.CreateStaffPayment;
using HMS.Modules.Staff.Application.Commands.CreateStaffWorkLog;
using HMS.Modules.Staff.Application.Commands.UpdateStaffMember;
using HMS.Modules.Staff.Application.Queries.GetStaffMember;
using HMS.Modules.Staff.Application.Queries.GetStaffWorkLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Staff.Api.Controllers;

[ApiController]
[Route("api/v1/staff")]
[Authorize]
public sealed class StaffController(ISender sender) : ControllerBase
{
    [HttpGet("{staffUid:guid}")]
    public async Task<IActionResult> Get(
        Guid staffUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffMemberQuery(
            staffUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{staffUid:guid}")]
    public async Task<IActionResult> Update(
        Guid staffUid,
        [FromBody] UpsertStaffMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateStaffMemberCommand(
            staffUid,
            request.StaffRoleUid,
            request.EmployeeNumber,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Email,
            request.EmploymentType,
            request.BasicSalary,
            request.DailyRate,
            request.HourlyRate,
            request.JoinedDate,
            request.LeftDate,
            request.Status,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpGet("{staffUid:guid}/work-logs")]
    public async Task<IActionResult> GetWorkLogs(
        Guid staffUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffWorkLogsQuery(
            staffUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("{staffUid:guid}/work-logs")]
    public async Task<IActionResult> CreateWorkLog(
        Guid staffUid,
        [FromBody] CreateStaffWorkLogRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateStaffWorkLogCommand(
            staffUid,
            request.WorkDate,
            request.StartTime,
            request.EndTime,
            request.HoursWorked,
            request.OvertimeHours,
            request.BookingUid,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpPost("{staffUid:guid}/payments")]
    public async Task<IActionResult> CreatePayment(
        Guid staffUid,
        [FromBody] CreateStaffPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateStaffPaymentCommand(
            staffUid,
            request.PeriodStart,
            request.PeriodEnd,
            request.BasicAmount,
            request.OvertimeAmount,
            request.BonusAmount,
            request.DeductionAmount,
            request.PaymentMethod,
            request.PaidAt,
            request.ReferenceNumber,
            request.Notes,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }
}
