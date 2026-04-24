namespace Lilliput.Api.Models;

public class RotaShift
{
    public Guid Id { get; set; }

    public DateTime Date { get; set; }

    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;

    public string AssignmentType { get; set; } = string.Empty;

    public string? Activity { get; set; }
    public string? GroupName { get; set; }

    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;

    public string? BookingId { get; set; }

    public string Status { get; set; } = "pending";
    public bool ConfirmedWorked { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}