using Core.Domain;

namespace Core.Commands.Commands;

public record ActivateAccountCommand(Guid Id, string Code)
    : RequireConfirmationCommand(Code, ConfirmableAction.AccountActivation) { }
