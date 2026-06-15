using Lilliput.Api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Lilliput.Api.Services;

public class InvoiceEmailService
{
    private readonly IConfiguration _configuration;
    private readonly InvoicePdfService _pdfService;

    public InvoiceEmailService(
        IConfiguration configuration,
        InvoicePdfService pdfService)
    {
        _configuration = configuration;
        _pdfService = pdfService;
    }

    public async Task SendInvoiceEmailAsync(Invoice invoice)
    {
        var fromName = _configuration["Email:FromName"] ?? "Lilliput Adventure Centre";
        var fromEmail = _configuration["Email:FromEmail"] ?? "";
        var smtpHost = _configuration["Email:SmtpHost"] ?? "";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "465");
        var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";

        if (string.IsNullOrWhiteSpace(invoice.SchoolEmail))
            throw new Exception("school email is missing");

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new Exception("from email is missing");

        if (string.IsNullOrWhiteSpace(smtpHost))
            throw new Exception("smtp host is missing");

        if (string.IsNullOrWhiteSpace(smtpUsername))
            throw new Exception("smtp username is missing");

        if (string.IsNullOrWhiteSpace(smtpPassword))
            throw new Exception("smtp password is missing");

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(invoice.SchoolEmail));
        message.Subject = $"Invoice {invoice.InvoiceNumber} - Lilliput Adventure Centre";

        var body = new BodyBuilder
        {
            TextBody =
$@"Hello,

Please find attached invoice {invoice.InvoiceNumber}.

Total due: €{invoice.TotalAmount:0.00}

Payment reference: {invoice.PaymentReference}

Kind regards,
Lilliput Adventure Centre"
        };

        body.Attachments.Add(
            $"{invoice.InvoiceNumber}.pdf",
            pdfBytes,
            ContentType.Parse("application/pdf")
        );

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