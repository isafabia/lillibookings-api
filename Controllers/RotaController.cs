using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Lilliput.Api.DTOs;
using Lilliput.Api.Services;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace Lilliput.Api.Controllers
{
    [ApiController]
    [Route("api/rota")]
    [Authorize]
    public class RotaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationEmailService _notificationEmailService;

        public RotaController(
            AppDbContext context,
            NotificationEmailService notificationEmailService)
        {
            _context = context;
            _notificationEmailService = notificationEmailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RotaShift>>> GetAll()
        {
            return await _context.RotaShifts
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        [HttpGet("weekly-responses")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetWeeklyResponses()
        {
            var today = DateTime.UtcNow.Date;

            var diffToMonday = today.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - today.DayOfWeek;

            var monday = today.AddDays(diffToMonday);
            var sunday = monday.AddDays(6);

            var shifts = await _context.RotaShifts
                .Where(s => s.Date.Date >= monday && s.Date.Date <= sunday)
                .ToListAsync();

            var pending = shifts
                .Where(s => s.Status.ToLower() == "pending")
                .Select(s => s.EmployeeName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var accepted = shifts
                .Where(s => s.Status.ToLower() == "accepted")
                .Select(s => s.EmployeeName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var declined = shifts
                .Where(s => s.Status.ToLower() == "declined")
                .Select(s => s.EmployeeName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            return Ok(new
            {
                pending,
                accepted,
                declined
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<RotaShift>> Create([FromBody] CreateRotaShiftDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.EmployeeName))
                {
                    return BadRequest(new { message = "employee name is required" });
                }

                if (string.IsNullOrWhiteSpace(dto.Date))
                {
                    return BadRequest(new { message = "date is required" });
                }

                if (!TryParseShiftDate(dto.Date, out var parsedDate))
                {
                    return BadRequest(new
                    {
                        message = "invalid date format",
                        receivedDate = dto.Date
                    });
                }

                var employeeId = string.IsNullOrWhiteSpace(dto.EmployeeId)
                    ? dto.EmployeeName.Trim().ToLower().Replace(" ", "-")
                    : dto.EmployeeId.Trim().ToLower();

                var shift = new RotaShift
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    EmployeeName = dto.EmployeeName.Trim(),
                    Date = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc),
                    StartTime = dto.StartTime ?? string.Empty,
                    EndTime = dto.EndTime ?? string.Empty,
                    AssignmentType = dto.AssignmentType ?? string.Empty,
                    Activity = string.IsNullOrWhiteSpace(dto.Activity) ? null : dto.Activity.Trim(),
                    GroupName = string.IsNullOrWhiteSpace(dto.GroupName) ? null : dto.GroupName.Trim(),
                    BookingId = string.IsNullOrWhiteSpace(dto.BookingId) ? null : dto.BookingId.Trim(),
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "pending" : dto.Status.Trim().ToLower(),
                    ConfirmedWorked = dto.ConfirmedWorked,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RotaShifts.Add(shift);
                await _context.SaveChangesAsync();

                try
                {
                    var employee = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id.ToString() == shift.EmployeeId);

                    if (employee != null && !string.IsNullOrWhiteSpace(employee.Email))
                    {
                        await _notificationEmailService.SendShiftRequestEmailAsync(
                            employee.Name,
                            employee.Email,
                            shift.Date.ToString("dd MMM yyyy"),
                            shift.StartTime,
                            shift.EndTime,
                            shift.GroupName,
                            shift.Activity
                        );
                    }
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"shift email notification failed: {emailEx.Message}");
                }

                return Ok(shift);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "error saving shift",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateShiftStatusDto dto)
        {
            var shift = await _context.RotaShifts
                .FirstOrDefaultAsync(x => x.Id.ToString() == id);

            if (shift == null)
                return NotFound();

            var status = dto.Status.Trim().ToLower();

            shift.Status = status;

            if (status == "accepted" || status == "declined" || status == "worked")
            {
                shift.RespondedAt = DateTime.UtcNow;
            }

            if (status == "worked")
            {
                shift.ConfirmedWorked = true;
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static bool TryParseShiftDate(string value, out DateTime parsedDate)
        {
            var formats = new[]
            {
                "yyyy-MM-dd",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
            };

            if (DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out parsedDate))
            {
                return true;
            }

            if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out parsedDate))
            {
                return true;
            }

            return false;
        }
    }
}