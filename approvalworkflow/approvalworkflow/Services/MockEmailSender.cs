using Microsoft.AspNetCore.Identity.UI.Services;

namespace approvalworkflow.Services;

public class MockEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.Run(() => Console.WriteLine($"Sent email to {email}: {subject} - {htmlMessage}"));
    }
}