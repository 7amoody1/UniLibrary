using System.Net;
using System.Net.Mail;
using BulkyBook.Models;

namespace BulkyBook.Utility;

public class Mailer : IMailer
{
    public Task SendEmailAsync(EmailPayload emailData) {
        var client = new SmtpClient("smtp.gmail.com", 587) {   
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
            "abood.reyhter@gmail.com", "yurs wuwe zpmb zpjl")
        };
 
        var message = new MailMessage(
            from: "abood.reyhter@gmail.com",
            to: emailData.Email,
            subject: emailData.Subject,
            body: emailData.Message
        )
        {
            IsBodyHtml = true  // Set this to true to send HTML emails
        };

        return client.SendMailAsync(message);
    }
}