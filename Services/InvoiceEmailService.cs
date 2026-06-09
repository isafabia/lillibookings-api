using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Lilliput.Api.Models;

namespace Lilliput.Api.Services;

public class InvoiceEmailService
{
    private readonly IConfiguration _configuration;
    private readonly InvoicePdfService _pdfService;
    private readonly HttpClient _httpClient;

    public InvoiceEmailService(
        IConfiguration configuration,
        InvoicePdfService pdfService,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _pdfService = pdfService;
        _httpClient = httpClient;
    }

    public async Task SendInvoiceEmailAsync(Invoice invoice)
    {
        var apiKey = _configuration["Resend:ApiKey"] ?? "";
        var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
        var fromName = _configuration["Resend:FromName"] ?? "Lilliput Adventure Centre";
        var replyToEmail = _configuration["Resend:ReplyToEmail"] ?? fromEmail;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("resend api key is missing");

        if (string.IsNullOrWhiteSpace(invoice.SchoolEmail))
            throw new Exception("school email is missing");

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
        var pdfBase64 = Convert.ToBase64String(pdfBytes);

        var payload = new
        {
            from = $"{fromName} <{fromEmail}>",
            reply_to = replyToEmail,
            to = new[] { invoice.SchoolEmail },
            subject = $"Invoice {invoice.InvoiceNumber} - Lilliput Adventure Centre",
            text =
$@"Hello,

Please find attached invoice {invoice.InvoiceNumber}.

Total due: €{invoice.TotalAmount:0.00}

Payment reference: {invoice.PaymentReference}

Kind regards,
Lilliput Adventure Centre",
            attachments = new[]
            {
                new
                {
                    filename = $"{invoice.InvoiceNumber}.pdf",
                    content = pdfBase64
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.resend.com/emails"
        );

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"resend failed: {response.StatusCode} - {responseBody}");
        }
    }
}