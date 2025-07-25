using Core.Domain;

namespace Core.Ports;

public interface AccountsRepository
{
    Task<Account?> FindByEmail(string email);
    Task<Account?> FindById(Guid id);
    Task<Account> FindByIdOrFail(Guid id);
    Task Create(Account account);
    Task Update(Account account);
}
