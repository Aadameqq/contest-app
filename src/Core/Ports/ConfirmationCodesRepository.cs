using Core.Domain;

namespace Core.Ports;

public interface ConfirmationCodesRepository
{
    Task Create(ConfirmationCode confirmationCode);
    Task<ConfirmationCode?> FindByCode(string code, ConfirmableAction action);
    Task<ConfirmationCode?> FindByCode(string code);
    Task<ConfirmationCode?> FindByAccount(Account account, ConfirmableAction action);
    Task Delete(ConfirmationCode confirmationCode);
    Task Update(ConfirmationCode confirmationCode);
    string GenerateCode();
}
