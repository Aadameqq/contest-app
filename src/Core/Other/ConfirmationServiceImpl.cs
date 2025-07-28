using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Other;

public class ConfirmationServiceImpl(
    ConfirmationCodeEmailSender emailSender,
    DateTimeProvider dateTimeProvider,
    UnitOfWork uow
) : ConfirmationService
{
    public async Task<Result> BeginConfirmation(Account account, ConfirmableAction action)
    {
        var repository = uow.GetConfirmationCodesRepository();

        var found = await repository.FindByAccount(account, action);

        var code = repository.GenerateCode();

        if (found is not null)
        {
            var result = found.Refresh(code, dateTimeProvider.Now());

            if (result is { IsFailure: true, Exception: TooManyAttempts })
            {
                return result.Exception;
            }

            if (result.IsSuccess)
            {
                await repository.Update(found);
                await uow.Flush();
                await emailSender.Send(account, found);
                return Result.Success();
            }
        }

        var confirmationCode = new ConfirmationCode(account, code, dateTimeProvider.Now(), action);

        await repository.Create(confirmationCode);
        await uow.Flush();

        await emailSender.Send(account, confirmationCode);

        return Result.Success();
    }

    public async Task<Result<Account>> Confirm(
        string code,
        ConfirmableAction action,
        Account account
    )
    {
        var codesRepository = uow.GetConfirmationCodesRepository();
        var confirmationCode = await codesRepository.FindByCode(code, action);

        if (confirmationCode is null)
        {
            return new NoSuch();
        }

        if (account is null || !confirmationCode.IsOwner(account))
        {
            return new NoSuch();
        }

        await codesRepository.Delete(confirmationCode);
        await uow.Flush();

        if (confirmationCode.HasExpired(dateTimeProvider.Now()))
        {
            return new Expired();
        }

        return account;
    }
}
