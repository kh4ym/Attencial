using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser is not null)
        {
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = "Email already registered"
            });
        }

        // Hash the password — never store plain text passwords
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // If registering as a Student, also create the Student profile
        if (request.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
        {
            var student = new Student
            {
                UserId = user.Id,
                FullName = request.FullName,
                RollNumber = request.RollNumber,
                EnrollmentStatus = "Pending"
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Registration successful"
        });
    }

    // POST /api/auth/login.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // Verify password against stored hash
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }

        // Build the JWT token
        var token = GenerateJwtToken(user);

        return Ok(new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role
            }
        });
    }

    // GET /api/auth/me  — protected endpoint
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new { email, role }
        });
    }

    // Private helper — builds the JWT token
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(jwtSettings["ExpiryMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
