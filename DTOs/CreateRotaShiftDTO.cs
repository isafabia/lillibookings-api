namespace Lilliput.Api.DTOs
{
    public class CreateRotaShiftDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;

        public string Date { get; set; } = string.Empty;

        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;

        public string AssignmentType { get; set; } = string.Empty;

        public string? Activity { get; set; }
        public string? GroupName { get; set; }
        public string? BookingId { get; set; }

        public string Status { get; set; } = "pending";
        public bool ConfirmedWorked { get; set; } = false;
    }
}