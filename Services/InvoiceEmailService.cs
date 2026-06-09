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
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["Email:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["Email:SmtpPassword"] ?? "";

        if (string.IsNullOrWhiteSpace(fromEmail) ||
            string.IsNullOrWhiteSpace(smtpHost) ||
            string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword))
        {
            throw new Exception("email settings are missing");
        }

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(invoice.SchoolEmail));
        message.Subject = $"Invoice {invoice.InvoiceNumber} - Lilliput Adventure Centre";

        var builder = new BodyBuilder
        {
            TextBody =
$@"Hello,

Please find attached invoice {invoice.InvoiceNumber} for your recent visit to Lilliput Adventure Centre.

Total due: €{invoice.TotalAmount:0.00}

Payment reference: {invoice.PaymentReference}

Kind regards,
Lilliput Adventure Centre"
        };

        builder.Attachments.Add(
            $"{invoice.InvoiceNumber}.pdf",
            pdfBytes,
            ContentType.Parse("application/pdf")
        );

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(smtpUsername, smtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}