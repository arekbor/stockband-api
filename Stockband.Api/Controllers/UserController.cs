using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stockband.Application.Features.UserFeatures.Commands.RegisterUser;
using Stockband.Application.Features.UserFeatures.Commands.RevokeToken;
using Stockband.Application.Features.UserFeatures.Commands.UpdatePassword;
using Stockband.Application.Features.UserFeatures.Commands.UpdateRole;
using Stockband.Application.Features.UserFeatures.Commands.UpdateUser;
using Stockband.Application.Features.UserFeatures.Queries.GetUserById;
using Stockband.Application.Features.UserFeatures.Queries.LoginUser;
using Stockband.Application.Features.UserFeatures.Queries.RefreshToken;
using Stockband.Domain.Common;

namespace Stockband.Api.Controllers;

[Authorize]
[ApiController]
public class UserController:ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [Route("/user/{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        BaseResponse<GetUserByIdQueryViewModel> response =  await _mediator.Send(new GetUserByIdQuery(id));

        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/user/register")]
    public async Task<IActionResult> UserRegister(RegisterUserCommand command)
    {
        BaseResponse response = await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/user/login")]
    public async Task<IActionResult> UserLogin(LoginUserQuery query)
    {
        BaseResponse<LoginUserQueryViewModel> response = await _mediator.Send(query);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/user/refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenQuery query)
    {
        BaseResponse<RefreshTokenQueryViewModel> response = await _mediator.Send(query);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPost]
    [Route("/user/revoke")]
    public async Task<IActionResult> RevokeRefreshToken(RevokeTokenCommand command)
    {
        BaseResponse response = await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPut]
    [Route("/user/password")]
    public async Task<IActionResult> UserUpdatePassword(UpdatePasswordCommand command)
    {
        BaseResponse response =  await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPut]
    [Route("/user/update")]
    public async Task<IActionResult> UserUpdate(UpdateUserCommand command)
    {
        BaseResponse response = await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [Route("/user/role")]
    public async Task<IActionResult> UserRole(UpdateRoleCommand command)
    {
        BaseResponse response = await _mediator.Send(command);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}