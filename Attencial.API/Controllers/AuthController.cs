using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Constants;
using Attencial.Shared.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IDistributedCache _cache;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(
        AppDbContext context, 
        IConfiguration config, 
        IDistributedCache cache,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _context = context;
        _config = config;
        _cache = cache;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validationResult = await _registerValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedRole = request.Role.Equals(AppConstants.Roles.Professor, StringComparison.OrdinalIgnoreCase)
            ? AppConstants.Roles.Professor
            : AppConstants.Roles.Student;

        // Check if email already exists
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

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
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = normalizedRole
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

	    // Create the matching profile in the same transaction so auth/profile state cannot split.
	    if (normalizedRole == AppConstants.Roles.Student)
	    {
	        _context.Students.Add(new Student
	        {
	            UserId = user.Id,
	            FullName = request.FullName.Trim(),
	            RollNumber = request.RollNumber.Trim(),
	            EnrollmentStatus = "Pending"
	        });
	    }
	    else if (normalizedRole == AppConstants.Roles.Professor)
	    {
	        _context.Professors.Add(new Professor
	        {
	            UserId = user.Id,
	            FullName = string.IsNullOrWhiteSpace(request.FullName) ? normalizedEmail : request.FullName.Trim(),
	            Department = string.Empty
	        });
	    }

	    await _context.SaveChangesAsync();
	    await transaction.CommitAsync();

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
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var lockoutKey = $"login:lockout:{normalizedEmail}";
        var failuresKey = $"login:failures:{normalizedEmail}";

        // 1. Check if user is locked out
        var isLocked = await _cache.GetStringAsync(lockoutKey);
        if (isLocked is not null)
        {
            return StatusCode(429, new ApiResponse<string>
            {
                Success = false,
                Message = "Too many failed login attempts. Your account is locked for 5 minutes."
            });
        }

        // Find user by email
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        // Verify password against stored hash
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            // Fetch current failures
            var failuresStr = await _cache.GetStringAsync(failuresKey);
            int failures = 0;
            if (failuresStr is not null)
            {
                int.TryParse(failuresStr, out failures);
            }

            failures++;

            if (failures >= 10)
            {
                // Lockout account for 5 minutes
                var lockoutOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(lockoutKey, "locked", lockoutOptions);
                await _cache.RemoveAsync(failuresKey);

                // Insert AbuseLog
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var abuseLog = new AbuseLog
                {
                    SessionId = null,
                    StudentId = null,
                    AbuseType = AppConstants.AbuseTypes.BruteForceLoginLockout,
                    Details = $"Account locked out due to 10 consecutive failed login attempts. Tried email: {request.Email}",
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AbuseLogs.Add(abuseLog);
                await _context.SaveChangesAsync();

                return StatusCode(429, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Too many failed login attempts. Your account is locked for 5 minutes."
                });
            }
            else
            {
                // Store failures with 15-minute sliding/absolute expiration
                var failureOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };
                await _cache.SetStringAsync(failuresKey, failures.ToString(), failureOptions);

                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }
        }

        // Clear failures on successful login
        await _cache.RemoveAsync(failuresKey);

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
