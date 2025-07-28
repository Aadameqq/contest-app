using Core.Commands.Commands;
using Core.Domain;
using Core.Ports;

namespace Core.Commands;

public abstract class InitializeConfirmationCommandHandler<TCommand>(
    ConfirmationService confirmationService
) : CommandHandler<TCommand>
    where TCommand : InitializeConfirmationCommand
{
    public async Task<Result> Handle(TCommand cmd, CancellationToken cancellationToken)
    {
        var result = await Prepare(cmd);

        if (result.IsFailure)
        {
            return result.Exception;
        }

        return await confirmationService.BeginConfirmation(result.Value, cmd.Action);
    }

    protected abstract Task<Result<Account>> Prepare(TCommand command);
}
