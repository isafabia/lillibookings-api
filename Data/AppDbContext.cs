using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Models;

namespace Lilliput.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RotaShift> RotaShifts => Set<RotaShift>();
}