using BulkyBook.Models;

namespace BulkyBook.Utility;

public interface IMailer
{
    Task SendEmailAsync(EmailPayload emailData);
}