namespace Lilliput.Api.Models;

public class Booking
{
    public Guid Id { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Activity { get; set; } = string.Empty;
    public int KidsCount { get; set; }
    public int TeachersCount { get; set; }
    public string MedicalNotes { get; set; } = string.Empty;
}