using System.Net;
using System.Net.Mail;
using MyEstore.Services.Interfaces;

namespace MyEstore.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host        = _configuration["Email:SmtpHost"]        ?? throw new InvalidOperationException("Email:SmtpHost is missing.");
        var port        = int.Parse(_configuration["Email:SmtpPort"]    ?? "587");
        var useSsl      = bool.Parse(_configuration["Email:UseSsl"]     ?? "true");
        var username    = _configuration["Email:Username"]        ?? throw new InvalidOperationException("Email:Username is missing.");
        var password    = _configuration["Email:Password"]        ?? throw new InvalidOperationException("Email:Password is missing.");
        var fromAddress = _configuration["Email:FromAddress"]     ?? username;
        var fromName    = _configuration["Email:FromDisplayName"] ?? "MyEstore";

        using var client = new SmtpClient(host, port)
        {
            Credentials  = new NetworkCredential(username, password),
            EnableSsl    = useSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(fromAddress, fromName),
            Subject    = subject,
            Body       = body,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(to));

        try
        {
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To} | Subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} | Subject: {Subject}", to, subject);
            throw;
        }
    }

    public Task SendOtpEmailAsync(string to, string otp)
    {
                var safeOtp = WebUtility.HtmlEncode(otp);
                var body = BuildEmailLayout(
                        title: "Your One-Time Verification Code",
                        subtitle: "Use this code to finish signing in securely.",
                        contentHtml: $@"
                                <p style=""margin:0 0 16px;color:#334155;font-size:15px;line-height:1.7;"">
                                        Use the code below to verify your request:
                                </p>
                                <div style=""margin:0 0 18px;padding:16px;background:linear-gradient(135deg,#f8fafc,#eef2ff);border:1px solid #e2e8f0;border-radius:14px;text-align:center;"">
                                        <span style=""display:inline-block;letter-spacing:8px;font-size:30px;font-weight:800;color:#0f172a;font-family:'Trebuchet MS',Verdana,sans-serif;"">{safeOtp}</span>
                                </div>
                                <p style=""margin:0 0 10px;color:#475569;font-size:14px;line-height:1.7;"">
                                        This code expires in <strong>5 minutes</strong>. Do not share this code with anyone.
                                </p>
                                <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.7;"">
                                        If you did not request this, you can ignore this message.
                                </p>",
                        footerNote: "MyEstore Security Team");

                return SendEmailAsync(to, "MyEstore - Your OTP Code", body);
    }

    public Task SendVerificationEmailAsync(string to, string firstName, string verificationLink)
    {
                var safeFirstName = WebUtility.HtmlEncode(firstName);
                var safeLink = WebUtility.HtmlEncode(verificationLink);
                var body = BuildEmailLayout(
                        title: $"Welcome, {safeFirstName}",
                        subtitle: "Your account is almost ready.",
                        contentHtml: $@"
                                <p style=""margin:0 0 16px;color:#334155;font-size:15px;line-height:1.7;"">
                                        Thanks for joining MyEstore. Please verify your email to activate your account.
                                </p>
                                <div style=""margin:0 0 18px;text-align:center;"">
                                        <a href=""{safeLink}"" style=""display:inline-block;padding:12px 22px;background:#0f766e;color:#ffffff;text-decoration:none;font-weight:700;border-radius:10px;font-size:14px;"">Verify Email Address</a>
                                </div>
                                <p style=""margin:0 0 10px;color:#475569;font-size:14px;line-height:1.7;"">
                                        This verification link expires in <strong>24 hours</strong>.
                                </p>
                                <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.7;"">
                                        If the button does not work, copy and paste this link in your browser:<br />
                                        <a href=""{safeLink}"" style=""color:#0f766e;text-decoration:underline;word-break:break-all;"">{safeLink}</a>
                                </p>",
                        footerNote: "MyEstore Team");

                return SendEmailAsync(to, "MyEstore - Verify Your Email", body);
    }

        private static string BuildEmailLayout(string title, string subtitle, string contentHtml, string footerNote)
        {
                return $@"
<!doctype html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>{title}</title>
</head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Verdana,Segoe UI,Tahoma,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""padding:28px 12px;background:#f1f5f9;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:620px;background:#ffffff;border:1px solid #e2e8f0;border-radius:16px;overflow:hidden;"">
                    <tr>
                        <td style=""padding:24px;background:linear-gradient(135deg,#0f172a,#0f766e);color:#ffffff;"">
                            <div style=""font-size:12px;letter-spacing:1.2px;text-transform:uppercase;opacity:0.9;font-weight:700;"">MyEstore</div>
                            <h1 style=""margin:10px 0 8px;font-size:24px;line-height:1.3;font-family:Georgia,'Times New Roman',serif;"">{title}</h1>
                            <p style=""margin:0;font-size:14px;line-height:1.6;opacity:0.95;"">{subtitle}</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:24px 24px 20px;"">
                            {contentHtml}
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:14px 24px 22px;border-top:1px solid #e2e8f0;color:#64748b;font-size:12px;line-height:1.7;"">
                            Sent by {footerNote}<br />
                            This is an automated email, please do not reply.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }
}
