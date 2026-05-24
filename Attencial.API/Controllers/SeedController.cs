using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Attencial.API.Controllers;

/// <summary>
/// Temporary controller to seed missing Student profiles.
/// Can be removed once a proper registration page with student fields exists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeedController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a Student profile for the currently logged-in user.
    /// POST /api/seed/create-student-profile
    /// Body: { "fullName": "John Doe", "rollNumber": "241871" }
    /// </summary>
    [HttpPost("create-student-profile")]
    [Authorize]
    public async Task<IActionResult> CreateStudentProfile([FromBody] CreateStudentProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if student already exists
        var existing = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        if (existing is not null)
        {
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = $"Student profile already exists (Id: {existing.Id}, Name: {existing.FullName})"
            });
        }

        var student = new Student
        {
            UserId = userId,
            FullName = request.FullName,
            RollNumber = request.RollNumber,
            EnrollmentStatus = "Pending"
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Student profile created! StudentId: {student.Id}"
        });
    }
}

public class CreateStudentProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
}
