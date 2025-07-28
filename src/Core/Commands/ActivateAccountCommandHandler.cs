using Core.Commands.Commands;
using Core.Domain;
using Core.Ports;

namespace Core.Commands;

public class ActivateAccountCommandHandler(ConfirmationService confirmationService, UnitOfWork uow)
    : RequireConfirmationCommandHandler<ActivateAccountCommand>(confirmationService)
{
    protected override async Task<Result<Account>> Prepare(ActivateAccountCommand cmd)
    {
        var accountsRepository = uow.GetAccountsRepository();
        return await uow.FailIfNull(() => accountsRepository.FindById(cmd.Id));
    }

    protected override async Task<Result> HandleWithConfirmation(
        Account account,
        ActivateAccountCommand cmd
    )
    {
        var accountsRepository = uow.GetAccountsRepository();

        account.Activate();

        await accountsRepository.Update(account);
        await uow.Flush();

        return Result.Success();
    }
}
