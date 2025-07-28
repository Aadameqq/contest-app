using Core.Commands.Commands;
using Core.Commands.Outputs;
using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.Commands;

public class LogInWithOAuthCommandHandler(
    OAuthStateTokenService stateTokensService,
    OAuthServiceFactory factory,
    UnitOfWork uow,
    SessionCreator sessionCreator
) : OAuthCommandHandler<LogInWithOAuthCommand, TokenPairOutput>(stateTokensService, factory)
{
    protected override async Task<Result<TokenPairOutput>> HandleWithAuth(
        OAuthUser oAuthUser,
        LogInWithOAuthCommand cmd
    )
    {
        var connectionsRepository = uow.GetOAuthConnectionsRepository();

        var connection = await connectionsRepository.Find(oAuthUser.OAuthId, oAuthUser.Provider);

        if (connection is null)
        {
            return await HandleAccountMissing(oAuthUser);
        }

        return await HandleAccountExists(connection);
    }

    private async Task<Result<TokenPairOutput>> HandleAccountMissing(OAuthUser oAuthUser)
    {
        if (!oAuthUser.IsEmailVerified)
        {
            return new OAuthEmailNotVerified();
        }

        var accountsRepository = uow.GetAccountsRepository();

        var found = await accountsRepository.FindByEmail(oAuthUser.Email);

        if (found is not null)
        {
            return new AlreadyExists<Account>();
        }

        var account = new Account(oAuthUser.UserName, oAuthUser.Email);
        var connection = new OAuthConnection(account, oAuthUser.Provider, oAuthUser.OAuthId);

        var connectionsRepository = uow.GetOAuthConnectionsRepository();

        var result = sessionCreator.CreateSession(account);

        await accountsRepository.Create(account);
        await connectionsRepository.Create(connection);

        await uow.Flush();

        return result.Value;
    }

    private async Task<Result<TokenPairOutput>> HandleAccountExists(OAuthConnection connection)
    {
        var accountsRepository = uow.GetAccountsRepository();

        var account = await uow.FailIfNull(() => accountsRepository.FindById(connection.AccountId));

        var result = sessionCreator.CreateSession(account);

        await accountsRepository.Update(account);
        await uow.Flush();

        return result.Value;
    }
}
