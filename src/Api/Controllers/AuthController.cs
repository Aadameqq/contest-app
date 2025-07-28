using Api.Auth;
using Api.Controllers.Dtos;
using Api.Dtos;
using Core.Commands.Commands;
using Core.Domain;
using Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TokenPairResponse>> LogIn([FromBody] LogInBody body)
    {
        var result = await mediator.Send(new LogInCommand(body.Email, body.Password));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.Unauthorized(),
                InvalidCredentials _ => ApiResponse.Unauthorized(),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpPost("oAuth")]
    public async Task<ActionResult<TokenPairResponse>> LogInWithOAuth([FromBody] OAuthBody body)
    {
        var result = await mediator.Send(
            new LogInWithOAuthCommand(body.StateToken, body.StateId, body.Code)
        );

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                InvalidOAuthCode _ => ApiResponse.Forbid("Given code is invalid or has expired"),
                InvalidState _ => ApiResponse.Forbid(),
                InvalidOAuthProvider _ => ApiResponse.BadRequest("Invalid oAuth provider"),
                OAuthProviderConnectionFailure _ => ApiResponse.ServiceUnavailable(
                    "Failed to receive a valid response from the external provider"
                ),
                AlreadyExists<Account> => ApiResponse.Conflict(
                    "Account with this email already exists"
                ),
                OAuthEmailNotVerified => ApiResponse.UnprocessableEntity(
                    "Your email address is not verified with the oAuth provider. Please verify it in oAuth provider account to continue"
                ),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpDelete]
    [RequireAuth]
    [OptionalActivation]
    public async Task<IActionResult> LogOut([FromAuth] AuthorizedUser authUser)
    {
        var result = await mediator.Send(new LogOutCommand(authUser.UserId, authUser.SessionId));

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return ApiResponse.Unauthorized();
        }

        return ApiResponse.Ok();
    }

    [HttpPut]
    public async Task<ActionResult<TokenPairResponse>> RefreshTokens(
        [FromBody] RefreshTokensBody body
    )
    {
        var result = await mediator.Send(new RefreshTokensCommand(body.RefreshToken));

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => ApiResponse.Unauthorized(),
                NoSuch<AuthSession> _ => ApiResponse.NotFound(),
                InvalidToken _ => ApiResponse.NotFound(),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}
