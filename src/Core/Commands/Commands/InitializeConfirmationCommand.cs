using Core.Domain;

namespace Core.Commands.Commands;

public record InitializeConfirmationCommand(ConfirmableAction Action) : Command { }
