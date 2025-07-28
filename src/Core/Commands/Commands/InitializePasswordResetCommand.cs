using Core.Domain;

namespace Core.Commands.Commands;

public record InitializePasswordResetCommand(string Email)
    : InitializeConfirmationCommand(ConfirmableAction.PasswordReset) { }
