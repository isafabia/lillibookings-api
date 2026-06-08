namespace Lilliput.Api.Models;

public class Invoice
{
    public Guid Id { get; set; }

    public Guid? BookingId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public string SchoolName { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public DateTime DateVisited { get; set; }

    public int ExpectedKidsCount { get; set; }
    public int ActualKidsCount { get; set; }
    public int TeachersCount { get; set; }

    public decimal PricePerChild { get; set; }
    public decimal PricePerTeacher { get; set; }
    public decimal ExtraCharges { get; set; }
    public decimal Discount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = "created";

    public string? PdfFileName { get; set; }
    public string? PdfUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }

    public string CompanyName { get; set; } = "Lilliput Adventure Centre";
public string CompanyAddress { get; set; } = "Lilliput, Mullingar, Co. Westmeath";
public string CompanyEmail { get; set; } = string.Empty;
public string CompanyPhone { get; set; } = string.Empty;

public string BankName { get; set; } = string.Empty;
public string Iban { get; set; } = string.Empty;
public string Bic { get; set; } = string.Empty;
public string PaymentReference { get; set; } = string.Empty;}