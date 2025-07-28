using System.Security.Claims;
using Core.Commands.Outputs;
using Core.Domain;
using Core.Dtos;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Other;

public class SystemAuthTokenService(IOptions<AuthOptions> authOptions) : AuthTokenService
{
    private const string SessionIdClaimType = "sessionId";
    private const string AccountIdClaimType = "accountId";
    private const string AccountRoleClaimType = "accountRole";
    private const string AccountIsActivatedClaimType = "isActivated";
    private const string TokenIdClaimType = "tokenId";

    private readonly JwtService accessTokenJwtService = new(
        authOptions.Value.AccessTokenSecret,
        true,
        authOptions.Value.AccessTokenLifetimeInMinutes
    );

    private readonly JwtService refreshTokenJwtService = new(
        authOptions.Value.RefreshTokenSecret,
        false
    );

    public TokenPairOutput CreateTokenPair(Account account, Guid sessionId, Guid tokenId)
    {
        var accessToken = CreateAccessToken(account, sessionId);
        var refreshToken = CreateRefreshToken(account, sessionId, tokenId);

        return new TokenPairOutput(accessToken, refreshToken);
    }

    public async Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken)
    {
        var principal = await accessTokenJwtService.FetchPayloadIfValid(accessToken);

        if (principal is null)
        {
            return null;
        }

        var userId = Guid.Parse(JwtService.GetClaim(principal, AccountIdClaimType));
        var sessionId = Guid.Parse(JwtService.GetClaim(principal, SessionIdClaimType));
        var role = JwtService.GetClaim(principal, AccountRoleClaimType);
        var isActivated = JwtService.GetClaim(principal, AccountIsActivatedClaimType);

        return new AccessTokenPayload(
            userId,
            sessionId,
            Role.ParseOrFail(role),
            bool.Parse(isActivated)
        );
    }

    public async Task<RefreshTokenPayload?> FetchRefreshTokenPayloadIfValid(string refreshToken)
    {
        var principal = await refreshTokenJwtService.FetchPayloadIfValid(refreshToken);

        if (principal is null)
        {
            return null;
        }

        var accountId = Guid.Parse(JwtService.GetClaim(principal, AccountIdClaimType));
        var sessionId = Guid.Parse(JwtService.GetClaim(principal, SessionIdClaimType));
        var tokenId = Guid.Parse(JwtService.GetClaim(principal, TokenIdClaimType));

        return new RefreshTokenPayload(accountId, tokenId, sessionId);
    }

    private string CreateAccessToken(Account account, Guid sessionId)
    {
        var claims = new List<Claim>
        {
            new(AccountIdClaimType, account.Id.ToString()),
            new(AccountRoleClaimType, account.Role.Name),
            new(SessionIdClaimType, sessionId.ToString()),
            new(AccountIsActivatedClaimType, account.HasBeenActivated().ToString()),
        };

        return accessTokenJwtService.SignToken(claims);
    }

    private string CreateRefreshToken(Account account, Guid sessionId, Guid tokenId)
    {
        var claims = new List<Claim>
        {
            new(AccountIdClaimType, account.Id.ToString()),
            new(SessionIdClaimType, sessionId.ToString()),
            new(TokenIdClaimType, tokenId.ToString()),
        };

        return refreshTokenJwtService.SignToken(claims);
    }
}
