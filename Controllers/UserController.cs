using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lilliput.Api.Data;
using Lilliput.Api.Models;
using Lilliput.Api.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lilliput.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public UsersController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
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
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ClearUsers()
    {
        var users = await _context.Users.ToListAsync();

        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "all users deleted"
        });
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedUsers()
    {
        if (await _context.Users.AnyAsync())
        {
            return BadRequest(new
            {
                message = "users already exist"
            });
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

        return Ok(new
        {
            message = "users seeded"
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == email
        );

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "invalid email or password"
            });
        }

        var passwordOk = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordOk)
        {
            return Unauthorized(new
            {
                message = "invalid email or password"
            });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new AuthResponse
        {
            Token = tokenString,
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            DayRate = user.DayRate
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<LoginResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new
            {
                message = "name is required"
            });
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "email is required"
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "password is required"
            });
        }

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email);

        if (existingUser)
        {
            return BadRequest(new
            {
                message = "a user with this email already exists"
            });
        }

        var role = string.IsNullOrWhiteSpace(request.Role)
            ? "employee"
            : request.Role.Trim().ToLower();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "user not found"
            });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "user deleted successfully"
        });
    }
}