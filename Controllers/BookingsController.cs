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
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookings), new { id = booking.Id }, booking);
    }
}