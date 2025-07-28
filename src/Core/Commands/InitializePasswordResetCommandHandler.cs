using Core.Commands.Commands;
using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Commands;

public class InitializePasswordResetCommandHandler(
    UnitOfWork uow,
    ConfirmationService confirmationService
) : InitializeConfirmationCommandHandler<InitializePasswordResetCommand>(confirmationService)
{
    protected override async Task<Result<Account>> Prepare(InitializePasswordResetCommand cmd)
    {
        var accountsRepository = uow.GetAccountsRepository();

        var found = await accountsRepository.FindByEmail(cmd.Email);

        if (found is null)
        {
            return new NoSuch<Account>();
        }

        if (!found.HasPassword())
        {
            return new NoPassword();
        }

        return found;
    }
}
