using Core.Domain;

namespace Core.Commands.Commands;

public record RequireConfirmationCommand<TOutput>(string Code, ConfirmableAction Action)
    : Command<TOutput> { }

public record RequireConfirmationCommand(string Code, ConfirmableAction Action) : Command { }
