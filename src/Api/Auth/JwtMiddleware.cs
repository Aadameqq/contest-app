using Core.Interactors;

namespace Api.Auth;

public class JwtMiddleware(RequestDelegate next, AccessTokenInteractor tokenInteractor)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var attribute = ctx.GetEndpoint()?.Metadata.OfType<RequireAuthAttribute>().FirstOrDefault();

        if (attribute is null)
        {
            await next(ctx);
            return;
        }

        const string tokenType = "Bearer ";

        var headerContent = ctx.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(headerContent) || !headerContent.StartsWith(tokenType))
        {
            await Results.Unauthorized().ExecuteAsync(ctx);
            return;
        }

        var token = headerContent[tokenType.Length..];

        var result = await tokenInteractor.GetAccessTokenPayload(token);

        if (result.IsFailure)
        {
            await Results.Unauthorized().ExecuteAsync(ctx);
            return;
        }

        ctx.Items["authorizedUser"] = new AuthorizedUser(
            result.Value.UserId,
            result.Value.SessionId
        );
        await next(ctx);
    }
}
