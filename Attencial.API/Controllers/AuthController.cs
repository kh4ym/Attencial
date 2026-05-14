using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;

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
}


[HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
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

        // Hash the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create the user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Registration successful"
        });
    }