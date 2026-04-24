using System.ComponentModel.DataAnnotations;

namespace Lilliput.Api.Models
{
   public class Booking
{
    public Guid Id { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public int KidsCount { get; set; }
    public int TeachersCount { get; set; }

    public string? MedicalNotes { get; set; }

    public string Status { get; set; } = "pending";

    // ✅ NEW FIELDS
    public string BookingType { get; set; } = "day-group";
    public int? Nights { get; set; }
}
}