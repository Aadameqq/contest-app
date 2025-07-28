using Core.Queries.Queries;
using MediatR;

namespace Api.Auth;

public class JwtMiddleware(RequestDelegate next, IMediator mediator)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var attribute = ctx.GetEndpoint()?.Metadata.OfType<RequireAuthAttribute>().FirstOrDefault();

        if (attribute is null)
        {
            await next(ctx);
            return;
        } // TODO: możeby logiki trochę do serwisu

        const string tokenType = "Bearer ";

        var headerContent = ctx.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(headerContent) || !headerContent.StartsWith(tokenType))
        {
            await ApiResponse.ApplyAsync(ctx, ApiResponse.Unauthorized());
            return;
        }

        var token = headerContent[tokenType.Length..];

        var result = await mediator.Send(new GetTokenPayloadQuery(token));

        if (result.IsFailure)
        {
            await ApiResponse.ApplyAsync(ctx, ApiResponse.Unauthorized());
            return;
        }

        var optionalActivationAttribute = ctx.GetEndpoint()
            ?.Metadata.OfType<OptionalActivationAttribute>()
            .FirstOrDefault();

        if (!result.Value.IsActivated && optionalActivationAttribute is null)
        {
            await ApiResponse.ApplyAsync(
                ctx,
                ApiResponse.Forbid("Account has not been activated yet")
            );
            return;
        }

        ctx.Items["authorizedUser"] = new AuthorizedUser(
            result.Value.UserId,
            result.Value.SessionId,
            result.Value.Role
        );
        await next(ctx);
    }
}
