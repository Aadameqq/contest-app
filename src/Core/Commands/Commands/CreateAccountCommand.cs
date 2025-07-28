using Core.Commands.Outputs;

namespace Core.Commands.Commands;

public record CreateAccountCommand(string UserName, string Email, string PlainPassword)
    : Command<TokenPairOutput>;
