using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;

namespace Lilliput.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        return await _context.Bookings.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetBooking(string id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id.ToString() == id);

        if (booking == null)
            return NotFound();

        return booking;
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        booking.Date = DateTime.SpecifyKind(booking.Date, DateTimeKind.Utc);
        booking.BookingType = NormalizeBookingType(booking.BookingType);

        if (booking.BookingType != "residential")
        {
            booking.Nights = null;
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBooking), new { id = booking.Id.ToString() }, booking);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(string id, Booking updatedBooking)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id.ToString() == id);

        if (booking == null)
            return NotFound();

        booking.GroupName = updatedBooking.GroupName;
        booking.Date = DateTime.SpecifyKind(updatedBooking.Date, DateTimeKind.Utc);
        booking.StartTime = updatedBooking.StartTime;
        booking.EndTime = updatedBooking.EndTime;
        booking.KidsCount = updatedBooking.KidsCount;
        booking.TeachersCount = updatedBooking.TeachersCount;
        booking.MedicalNotes = updatedBooking.MedicalNotes;
        booking.Status = updatedBooking.Status;
        booking.BookingType = NormalizeBookingType(updatedBooking.BookingType);
        booking.Nights = booking.BookingType == "residential" ? updatedBooking.Nights : null;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(string id)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id.ToString() == id);

        if (booking == null)
            return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
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