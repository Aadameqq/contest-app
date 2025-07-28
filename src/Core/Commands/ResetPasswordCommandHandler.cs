using Core.Commands.Commands;
using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Commands;

public class ResetPasswordCommandHandler(
    PasswordHasher passwordHasher,
    UnitOfWork uow,
    ConfirmationService confirmationService
) : RequireConfirmationCommandHandler<ResetPasswordCommand>(confirmationService)
{
    protected override async Task<Result<Account>> Prepare(ResetPasswordCommand cmd)
    {
        var confirmationCodeRepository = uow.GetConfirmationCodesRepository();
        var accountsRepository = uow.GetAccountsRepository();
        var foundCode = await confirmationCodeRepository.FindByCode(cmd.Code);

        if (foundCode is null)
        {
            return new NoSuch();
        }

        var found = await accountsRepository.FindById(foundCode.OwnerId);

        if (found is null)
        {
            return new NoSuch();
        }

        return found;
    }

    protected override async Task<Result> HandleWithConfirmation(
        Account account,
        ResetPasswordCommand cmd
    )
    {
        var accountsRepository = uow.GetAccountsRepository();

        var passwordHash = passwordHasher.HashPassword(cmd.NewPassword);

        account.ResetPassword(passwordHash);

        await accountsRepository.Update(account);
        await uow.Flush();

        return Result.Success();
    }
}
