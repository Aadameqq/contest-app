using Core.Domain;

namespace Core.Commands.Commands;

public record InitializeAccountActivationCommand(Guid Id)
    : InitializeConfirmationCommand(ConfirmableAction.AccountActivation);
