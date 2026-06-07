using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Lilliput.Api.Dtos;
using Lilliput.Api.Services;

namespace Lilliput.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly InvoicePdfService _pdfService;
    private readonly InvoiceEmailService _emailService;

    public InvoicesController(
        AppDbContext context,
        InvoicePdfService pdfService,
        InvoiceEmailService emailService)
    {
        _context = context;
        _pdfService = pdfService;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
    {
        var invoices = await _context.Invoices
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<Invoice?>> GetLatestInvoice()
    {
        var invoice = await _context.Invoices
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync();

        return Ok(invoice);
    }

    [HttpPost("create")]
    public async Task<ActionResult<Invoice>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var validationError = ValidateInvoiceRequest(request);

        if (validationError != null)
            return BadRequest(validationError);

        var total =
            (request.ActualKidsCount * request.PricePerChild) +
            (request.TeachersCount * request.PricePerTeacher) +
            request.ExtraCharges -
            request.Discount;

        if (total < 0)
            total = 0;

        var invoiceNumber = await GenerateInvoiceNumber();

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BookingId = request.BookingId,
            InvoiceNumber = invoiceNumber,

            CompanyName = "Lilliput Adventure Centre",
            CompanyAddress = "Lilliput, Mullingar, Co. Westmeath",
            CompanyEmail = "info@lilliput.ie",
            CompanyPhone = "",

            BankName = "",
            Iban = "",
            Bic = "",
            PaymentReference = invoiceNumber,

            SchoolName = request.SchoolName.Trim(),
            SchoolEmail = request.SchoolEmail.Trim().ToLower(),
            Location = request.Location.Trim(),

            DateVisited = DateTime.SpecifyKind(request.DateVisited.Date, DateTimeKind.Utc),

            ExpectedKidsCount = request.ExpectedKidsCount,
            ActualKidsCount = request.ActualKidsCount,
            TeachersCount = request.TeachersCount,

            PricePerChild = request.PricePerChild,
            PricePerTeacher = request.PricePerTeacher,
            ExtraCharges = request.ExtraCharges,
            Discount = request.Discount,

            TotalAmount = total,

            Notes = request.Notes.Trim(),
            Status = "created",
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        return Ok(invoice);
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(Guid id)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound("invoice not found");

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

        return File(
            pdfBytes,
            "application/pdf",
            $"{invoice.InvoiceNumber}.pdf"
        );
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound("invoice not found");

        try
        {
            await _emailService.SendInvoiceEmailAsync(invoice);

            invoice.Status = "sent";
            invoice.SentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "invoice sent successfully"
            });
        }
        catch (Exception ex)
        {
           Console.WriteLine(ex.ToString());
           
            invoice.Status = "failed";
            await _context.SaveChangesAsync();

            return BadRequest(new
            {
                message = "invoice could not be sent",
                error = ex.Message
            });
        }
    }

    private async Task<string> GenerateInvoiceNumber()
    {
        var year = DateTime.UtcNow.Year;

        var countThisYear = await _context.Invoices
            .CountAsync(i => i.CreatedAt.Year == year);

        var nextNumber = countThisYear + 1;

        return $"INV-{year}-{nextNumber:D4}";
    }

    private static string? ValidateInvoiceRequest(CreateInvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SchoolName))
            return "school name is required";

        if (string.IsNullOrWhiteSpace(request.SchoolEmail))
            return "school email is required";

        if (string.IsNullOrWhiteSpace(request.Location))
            return "location is required";

        if (request.ExpectedKidsCount < 0)
            return "expected kids count cannot be negative";

        if (request.ActualKidsCount < 0)
            return "actual kids count cannot be negative";

        if (request.TeachersCount < 0)
            return "teachers count cannot be negative";

        if (request.PricePerChild < 0)
            return "price per child cannot be negative";

        if (request.PricePerTeacher < 0)
            return "price per teacher cannot be negative";

        if (request.ExtraCharges < 0)
            return "extra charges cannot be negative";

        if (request.Discount < 0)
            return "discount cannot be negative";

        return null;
    }
}