using Core.Domain;

namespace Core.Ports;

public interface ConfirmationCodeEmailSender
{
    public Task Send(Account account, ConfirmationCode confirmationCode);
}
