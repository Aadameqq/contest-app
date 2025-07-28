using Core.Commands.Commands;
using Core.Commands.Outputs;
using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Commands;

public class LogInCommandHandler(
    UnitOfWork uow,
    PasswordVerifier passwordVerifier,
    SessionCreator sessionCreator
) : CommandHandler<LogInCommand, TokenPairOutput>
{
    public async Task<Result<TokenPairOutput>> Handle(LogInCommand cmd, CancellationToken _)
    {
        var accountsRepository = uow.GetAccountsRepository();

        var account = await accountsRepository.FindByEmail(cmd.Email);
        if (account is null)
        {
            return new NoSuch<Account>();
        }

        if (!account.HasPassword())
        {
            return new NoPassword();
        }

        if (!passwordVerifier.Verify(cmd.Password, account.Password!))
        {
            return new InvalidCredentials();
        }

        var result = sessionCreator.CreateSession(account);

        await accountsRepository.Update(account);
        await uow.Flush();

        return result.Value;
    }
}
