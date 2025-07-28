using Core.Domain;

namespace Core.Commands.Commands;

public record ResetPasswordCommand(string Code, string NewPassword)
    : RequireConfirmationCommand(Code, ConfirmableAction.PasswordReset);
