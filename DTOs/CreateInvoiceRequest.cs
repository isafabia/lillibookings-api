namespace Lilliput.Api.Dtos;

public class CreateInvoiceRequest
{
    public Guid? BookingId { get; set; }

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

    public string Notes { get; set; } = string.Empty;
}