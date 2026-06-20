using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Lilliput.Api.Services;

public class NotificationEmailService
{
    private readonly IConfiguration _configuration;

    public NotificationEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendShiftRequestEmailAsync(
        string employeeName,
        string employeeEmail,
        string date,
        string startTime,
        string endTime,
        string? groupName,
        string? activity)
    {
        var fromName = _configuration["Email:FromName"] ?? "Lilliput Adventure Centre";
        var fromEmail = _configuration["Email:FromEmail"] ?? "";
        var smtpHost = _configuration["Email:SmtpHost"] ?? "";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";

        if (string.IsNullOrWhiteSpace(employeeEmail))
            throw new Exception("employee email is missing");

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new Exception("from email is missing");

        if (string.IsNullOrWhiteSpace(smtpHost))
            throw new Exception("smtp host is missing");

        if (string.IsNullOrWhiteSpace(smtpUsername))
            throw new Exception("smtp username is missing");

        if (string.IsNullOrWhiteSpace(smtpPassword))
            throw new Exception("smtp password is missing");

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(employeeEmail));
        message.Subject = "New Shift Request - Lilliput Adventure Centre";

        var body = new BodyBuilder
        {
            TextBody =
$@"Hi {employeeName},

You have received a new shift request.

Date: {date}
Time: {startTime} - {endTime}

Group: {groupName ?? "not specified"}
Activity: {activity ?? "not specified"}

Please log into the Lilliput app to accept or decline the shift.

Kind regards,
Lilliput Adventure Centre"
        };

        message.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        client.Timeout = 60000;

        var socketOption = smtpPort == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        await client.ConnectAsync(smtpHost, smtpPort, socketOption);
        await client.AuthenticateAsync(smtpUsername, smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}