namespace Core.Queries.Outputs;

public record AccountDetailsOutput(Guid Id, string UserName, string Email, bool Activated);
