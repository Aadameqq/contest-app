using Core.Domain;
using Core.Ports;

namespace Infrastructure.Smtp;

public class ConfirmationCodeEmailSenderImpl(EmailSender emailSender) : ConfirmationCodeEmailSender
{
    public Task Send(Account account, ConfirmationCode code)
    {
        return emailSender.Send(account.Email, "Action Confirmation", GetBody(account, code));
    }

    private string GetBody(Account account, ConfirmationCode code)
    {
        var html = File.ReadAllText(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Smtp",
                "templates",
                "confirmation.html"
            )
        );

        html = html.Replace("{userName}", account.UserName);
        html = html.Replace("{code}", code.Code);
        html = html.Replace("{action}", code.Action.Name);
        html = html.Replace("{life}", code.LifeSpan.ToString());

        return html;
    }
}
