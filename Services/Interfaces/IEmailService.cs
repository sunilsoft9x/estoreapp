namespace MyEstore.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendOtpEmailAsync(string to, string otp);
    Task SendVerificationEmailAsync(string to, string firstName, string verificationLink);
}
