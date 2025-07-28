using Api.Auth;
using Api.Controllers.Dtos;
using Core.Commands.Commands;
using Core.Domain;
using Core.Exceptions;
using Core.Queries.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(IMediator mediator) : ControllerBase
{
    [HttpPost("")]
    public async Task<ActionResult<TokenPairResponse>> Create([FromBody] CreateAccountBody body)
    {
        var result = await mediator.Send(
            new CreateAccountCommand(body.Username, body.Email, body.Password)
        );

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                AlreadyExists<Account> _ => ApiResponse.Conflict(
                    "Account with given email already exists"
                ),
                TooManyAttempts _ => ApiResponse.Cooldown(),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpGet("@me")]
    [RequireAuth]
    [OptionalActivation]
    public async Task<ActionResult<GetAuthenticatedUserResponse>> GetAuthenticated(
        [FromAuth] AuthorizedUser user
    )
    {
        var result = await mediator.Send(new GetCurrentAccountQuery(user.UserId));

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return ApiResponse.Unauthorized();
        }

        return new GetAuthenticatedUserResponse(result.Value);
    }

    [HttpDelete("@me/password")]
    public async Task<IActionResult> InitializeResetPassword(
        [FromBody] InitializeResetPasswordBody body
    )
    {
        var result = await mediator.Send(new InitializePasswordResetCommand(body.Email));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                TooManyAttempts => ApiResponse.Cooldown(),
                NoPassword => ApiResponse.BadRequest(
                    "Cannot reset password on non-password account"
                ),
                NoSuch<Account> => ApiResponse.BadRequest(
                    "Account with given email does not exist"
                ),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok("Password reset email sent");
    }

    [HttpPost("@me/activation")]
    [RequireAuth]
    [OptionalActivation]
    public async Task<IActionResult> InitializeActivation([FromAuth] AuthorizedUser user)
    {
        var result = await mediator.Send(new InitializeAccountActivationCommand(user.UserId));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                TooManyAttempts => ApiResponse.Cooldown(),
                AlreadyActivated => ApiResponse.Forbid("Account already activated"),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok("Activation email sent");
    }

    [HttpPost("@me/activation/{code}")]
    [RequireAuth]
    [OptionalActivation]
    public async Task<IActionResult> Activate(
        [FromRoute] string code,
        [FromAuth] AuthorizedUser user
    )
    {
        var result = await mediator.Send(new ActivateAccountCommand(user.UserId, code));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch _ => ApiResponse.NotFound("Code not found"),
                Expired _ => ApiResponse.Timeout("Given code has already expired"),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok("Account activated. Please refresh your access token");
    }

    [HttpDelete("@me/password/{code}")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordBody body,
        [FromRoute] string code
    )
    {
        var result = await mediator.Send(new ResetPasswordCommand(code, body.NewPassword));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch _ => ApiResponse.NotFound("Code not found"),
                Expired _ => ApiResponse.Timeout("Given code has already expired"),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok();
    }

    [HttpPost("{accountId}/role")]
    [RequireAuth]
    public async Task<IActionResult> AssignRole(
        [FromAuth] AuthorizedUser issuer,
        [FromRoute] string accountId,
        [FromBody] AssignRoleBody body,
        [FromAuth] AccessManager accessManager
    )
    {
        if (!accessManager.HasAnyRole(Role.Admin))
        {
            return ApiResponse.Forbid();
        }

        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            return ApiResponse.NotFound("Account not found");
        }

        if (!Role.TryParse(body.RoleName, out var role))
        {
            return ApiResponse.NotFound("Role not found");
        }

        var result = await mediator.Send(
            new AssignRoleCommand(issuer.UserId, parsedAccountId, role)
        );

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.NotFound("Account not found"),
                CannotManageOwn<Role> _ => ApiResponse.Forbid(
                    "Assigning a role to your own account is not permitted"
                ),
                RoleAlreadyAssigned _ => ApiResponse.Conflict(
                    "Account already assigned to role. Remove role before assigning"
                ),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok();
    }

    [HttpDelete("{accountId}/role")]
    [RequireAuth]
    public async Task<IActionResult> UnassignRole(
        [FromAuth] AuthorizedUser issuer,
        [FromRoute] string accountId,
        [FromAuth] AccessManager accessManager
    )
    {
        if (!accessManager.HasAnyRole(Role.Admin))
        {
            return ApiResponse.Forbid();
        }

        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            return ApiResponse.NotFound();
        }

        var result = await mediator.Send(new UnassignRoleCommand(issuer.UserId, parsedAccountId));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.NotFound(),
                CannotManageOwn<Role> _ => ApiResponse.Forbid(
                    "Unassigning a role from your own account is not permitted"
                ),
                _ => throw result.Exception,
            };
        }

        return ApiResponse.Ok();
    }
}
