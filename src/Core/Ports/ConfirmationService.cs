using Core.Domain;

namespace Core.Ports;

public interface ConfirmationService
{
    Task<Result> BeginConfirmation(Account account, ConfirmableAction action);

    Task<Result<Account>> Confirm(string code, ConfirmableAction action, Account account);
}
