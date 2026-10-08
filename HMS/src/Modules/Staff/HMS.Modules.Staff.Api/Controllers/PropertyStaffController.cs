using HMS.Modules.Staff.Api.Contracts;
using HMS.Modules.Staff.Application.Commands.CreateStaffMember;
using HMS.Modules.Staff.Application.Queries.GetStaffMembers;
using HMS.Modules.Staff.Application.Queries.GetStaffPayments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Modules.Staff.Api.Controllers;

[ApiController]
[Route("api/v1/properties/{propertyUid:guid}")]
[Authorize]
public sealed class PropertyStaffController(ISender sender) : ControllerBase
{
    [HttpGet("staff")]
    public async Task<IActionResult> Get(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffMembersQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost("staff")]
    public async Task<IActionResult> Create(
        Guid propertyUid,
        [FromBody] UpsertStaffMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateStaffMemberCommand(
            propertyUid,
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

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("staff-payments")]
    public async Task<IActionResult> GetPayments(
        Guid propertyUid,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStaffPaymentsQuery(
            propertyUid,
            User.GetRequiredSubject(),
            User.IsPlatformAdmin()), cancellationToken);

        return this.ToActionResult(result);
    }
}
