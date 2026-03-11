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

    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        booking.Date = DateTime.SpecifyKind(booking.Date, DateTimeKind.Utc);

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookings), new { id = booking.Id }, booking);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(Guid id, Booking updatedBooking)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        booking.GroupName = updatedBooking.GroupName;
        booking.Date = DateTime.SpecifyKind(updatedBooking.Date, DateTimeKind.Utc);
        booking.StartTime = updatedBooking.StartTime;
        booking.EndTime = updatedBooking.EndTime;
        booking.KidsCount = updatedBooking.KidsCount;
        booking.TeachersCount = updatedBooking.TeachersCount;
        booking.MedicalNotes = updatedBooking.MedicalNotes;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}