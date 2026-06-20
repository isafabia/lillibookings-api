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
        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _configuration["Email:FromName"],
                _configuration["Email:FromEmail"]
            ));

            message.To.Add(MailboxAddress.Parse(employeeEmail));

            message.Subject = "New Shift Request - Lilliput";

            var body = new BodyBuilder
            {
                TextBody =
$@"Hi {employeeName},

You have received a new shift request.

Date: {date}
Time: {startTime} - {endTime}

Group: {groupName ?? "Not specified"}
Activity: {activity ?? "Not specified"}

Please log into the Lilliput app to accept or decline the shift.

Kind regards,
Lilliput Adventure Centre"
            };

            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(
                _configuration["Email:SmtpHost"],
                int.Parse(_configuration["Email:SmtpPort"]!),
                SecureSocketOptions.SslOnConnect
            );

            await client.AuthenticateAsync(
                _configuration["Email:SmtpUsername"],
                _configuration["Email:SmtpPassword"]
            );

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Shift email failed: {ex.Message}");
        }
    }
}