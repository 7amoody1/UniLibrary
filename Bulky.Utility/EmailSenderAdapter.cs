using BulkyBook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BulkyBook.Utility;

public class EmailSenderAdapter : IEmailSender
{
    private readonly IMailer _mailer;

    public EmailSenderAdapter(IMailer mailer)
    {
        _mailer = mailer;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var emailPayload = new EmailPayload
        {
            Email = email,
            Subject = subject,
            Message = htmlMessage
        };

        return _mailer.SendEmailAsync(emailPayload);
    }
}