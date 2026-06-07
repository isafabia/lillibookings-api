using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Lilliput.Api.DTOs;
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

        public RotaController(AppDbContext context)
        {
            _context = context;
        }

        // logged-in users can view rota shifts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RotaShift>>> GetAll()
        {
            return await _context.RotaShifts
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .ToListAsync();
        }

        // only admin can create shifts
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

                var shift = new RotaShift
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = string.IsNullOrWhiteSpace(dto.EmployeeId)
                        ? dto.EmployeeName.Trim().ToLower().Replace(" ", "-")
                        : dto.EmployeeId.Trim().ToLower(),

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

        // logged-in users can update shift status
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

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsedDate))
            {
                return true;
            }

            return false;
        }
    }
}