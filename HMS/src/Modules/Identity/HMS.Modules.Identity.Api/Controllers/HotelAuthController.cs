using HMS.Modules.Identity.Api.Contracts;
using HMS.Modules.Identity.Application.Commands.LoginStaff;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/hotel/auth")]
    public sealed class HotelAuthController : ControllerBase
    {
        private readonly ISender _sender;

        public HotelAuthController(ISender sender)
        {
            _sender = sender;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(
            typeof(StaffLoginResponse),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<IActionResult> Login(
            StaffLoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new LoginStaffCommand(
                request.PropertyUid,
                request.Username,
                request.Password);

            var result = await _sender.Send(
                command,
                cancellationToken);

            if (result.IsSuccessful)
            {
                return Ok(result.Data);
            }

            return result.ErrorCode switch
            {
                "validation_error" => BadRequest(new
                {
                    result.ErrorCode,
                    result.ErrorMessage
                }),

                "account_locked" => StatusCode(
                    StatusCodes.Status423Locked,
                    new
                    {
                        result.ErrorCode,
                        result.ErrorMessage
                    }),

                "account_inactive" => Unauthorized(new
                {
                    result.ErrorCode,
                    result.ErrorMessage
                }),

                _ => Unauthorized(new
                {
                    result.ErrorCode,
                    result.ErrorMessage
                })
            };
        }
    }
}
