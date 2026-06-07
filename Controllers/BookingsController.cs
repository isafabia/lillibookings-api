using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Microsoft.AspNetCore.Authorization;

namespace Lilliput.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BookingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        return await _context.Bookings
            .OrderBy(b => b.Date)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetBooking(string id)
    {
        if (!Guid.TryParse(id, out var bookingId))
            return BadRequest("invalid booking id");

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return NotFound();

        return booking;
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        var validationError = ValidateBooking(booking);

        if (validationError != null)
            return BadRequest(validationError);

        booking.Id = booking.Id == Guid.Empty ? Guid.NewGuid() : booking.Id;
        booking.GroupName = booking.GroupName.Trim();
        booking.Location = booking.Location.Trim();
        booking.SchoolEmail = booking.SchoolEmail.Trim().ToLower();

        booking.Status = string.IsNullOrWhiteSpace(booking.Status)
            ? "pending"
            : booking.Status.Trim().ToLower();

        booking.Date = DateTime.SpecifyKind(booking.Date.Date, DateTimeKind.Utc);
        booking.BookingType = NormalizeBookingType(booking.BookingType);

        if (booking.BookingType != "residential")
        {
            booking.Nights = null;
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateBooking(string id, Booking updatedBooking)
    {
        if (!Guid.TryParse(id, out var bookingId))
            return BadRequest("invalid booking id");

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return NotFound();

        var validationError = ValidateBooking(updatedBooking);

        if (validationError != null)
            return BadRequest(validationError);

        booking.GroupName = updatedBooking.GroupName.Trim();
        booking.Location = updatedBooking.Location.Trim();
        booking.SchoolEmail = updatedBooking.SchoolEmail.Trim().ToLower();
        booking.Date = DateTime.SpecifyKind(updatedBooking.Date.Date, DateTimeKind.Utc);
        booking.StartTime = updatedBooking.StartTime;
        booking.EndTime = updatedBooking.EndTime;
        booking.KidsCount = updatedBooking.KidsCount;
        booking.TeachersCount = updatedBooking.TeachersCount;
        booking.MedicalNotes = updatedBooking.MedicalNotes;

        booking.Status = string.IsNullOrWhiteSpace(updatedBooking.Status)
            ? "pending"
            : updatedBooking.Status.Trim().ToLower();

        booking.BookingType = NormalizeBookingType(updatedBooking.BookingType);
        booking.Nights = booking.BookingType == "residential" ? updatedBooking.Nights : null;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteBooking(string id)
    {
        if (!Guid.TryParse(id, out var bookingId))
            return BadRequest("invalid booking id");

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return NotFound();

        var invoices = await _context.Invoices
            .Where(i => i.BookingId == booking.Id)
            .ToListAsync();

        _context.Invoices.RemoveRange(invoices);
        _context.Bookings.Remove(booking);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateBooking(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.GroupName))
            return "group name is required";

        if (string.IsNullOrWhiteSpace(booking.Location))
            return "location is required";

        if (string.IsNullOrWhiteSpace(booking.SchoolEmail))
            return "school email is required";

        if (booking.KidsCount < 0)
            return "kids count cannot be negative";

        if (booking.TeachersCount < 0)
            return "teachers count cannot be negative";

        return null;
    }

    private static string NormalizeBookingType(string? value)
    {
        var type = (value ?? "").Trim().ToLower();

        return type switch
        {
            "residential" => "residential",
            "day-group" => "day-group",
            "day group" => "day-group",
            "day_group" => "day-group",
            "daygroup" => "day-group",
            _ => "day-group"
        };
    }
}