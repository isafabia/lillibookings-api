using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Lilliput.Api.Dtos;

namespace Lilliput.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoginResponse>>> GetUsers()
    {
        var users = await _context.Users
            .OrderBy(u => u.Name)
            .Select(u => new LoginResponse
            {
                Id = u.Id.ToString(),
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                DayRate = u.DayRate
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearUsers()
    {
        var users = await _context.Users.ToListAsync();
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();

        return Ok("all users deleted");
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedUsers()
    {
        if (await _context.Users.AnyAsync())
        {
            return BadRequest("users already exist");
        }

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Franky",
                Email = "franky@lilliput.ie",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("franky123"),
                Role = "admin",
                DayRate = 100
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Samuel",
                Email = "samuel@lilliput.ie",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("samuel123"),
                Role = "employee",
                DayRate = 80
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "Employee 2",
                Email = "employee2@lilliput.ie",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
                Role = "employee",
                DayRate = 50
            }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        return Ok("users seeded");
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == email
        );

        if (user == null)
        {
            return Unauthorized("invalid email or password");
        }

        var passwordOk = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordOk)
        {
            return Unauthorized("invalid email or password");
        }

        return Ok(new LoginResponse
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DayRate = user.DayRate
        });
    }

    [HttpPost]
    public async Task<ActionResult<LoginResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var email = request.Email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("name is required");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("email is required");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("password is required");
        }

        var existingUser = await _context.Users.AnyAsync(u => u.Email.ToLower() == email);

        if (existingUser)
        {
            return BadRequest("a user with this email already exists");
        }

        var role = string.IsNullOrWhiteSpace(request.Role)
            ? "employee"
            : request.Role.Trim().ToLower();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            DayRate = request.DayRate
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new LoginResponse
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DayRate = user.DayRate
        });
    }
}