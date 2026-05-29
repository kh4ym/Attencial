using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Attencial.API.Controllers;

/// <summary>
/// Temporary seeding controller for development/testing.
/// Creates student profiles, professor profiles, and courses without needing a full UI.
/// Can be removed once proper admin flows exist.
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

    // ── GET /api/seed/me ─────────────────────────────────────────────────────
    // Quick debugging endpoint: shows who you are and what profiles exist.
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var email  = User.FindFirstValue(ClaimTypes.Email) ?? "(no email claim)";
        var role   = User.FindFirstValue(ClaimTypes.Role)  ?? "(no role claim)";

        var student   = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new
            {
                UserId      = userId,
                Email       = email,
                Role        = role,
                StudentProfile = student is null ? null : new
                {
                    student.Id,
                    student.FullName,
                    student.RollNumber,
                    student.EnrollmentStatus
                },
                ProfessorProfile = professor is null ? null : new
                {
                    professor.Id,
                    professor.FullName,
                    professor.Department
                }
            }
        });
    }

    // ── POST /api/seed/create-student-profile ─────────────────────────────────
    // Body: { "fullName": "John Doe", "rollNumber": "241871" }
    [HttpPost("create-student-profile")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreateStudentProfile([FromBody] CreateStudentProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existing = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        if (existing is not null)
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = $"Student profile already exists (Id: {existing.Id}, Name: {existing.FullName})"
            });

        var student = new Student
        {
            UserId           = userId,
            FullName         = request.FullName,
            RollNumber       = request.RollNumber,
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

    // ── POST /api/seed/create-professor-profile ───────────────────────────────
    // Body: { "fullName": "Dr. Smith", "department": "Computer Science" }
    [HttpPost("create-professor-profile")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> CreateProfessorProfile([FromBody] CreateProfessorProfileRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var existing = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);
        if (existing is not null)
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = $"Professor profile already exists (Id: {existing.Id}, Name: {existing.FullName})"
            });

        var professor = new Professor
        {
            UserId     = userId,
            FullName   = request.FullName,
            Department = request.Department
        };

        _context.Professors.Add(professor);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Professor profile created! ProfessorId: {professor.Id}"
        });
    }

    // ── POST /api/seed/create-course ──────────────────────────────────────────
    // Creates a course owned by the logged-in professor.
    // Body: { "name": "Software Engineering", "courseCode": "CS-401" }
    [HttpPost("create-course")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);
        if (professor is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "No professor profile found for this account. Call POST /api/seed/create-professor-profile first."
            });

        // Prevent duplicate course codes for this professor
        var duplicate = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseCode == request.CourseCode && c.ProfessorId == professor.Id);

        if (duplicate is not null)
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = $"Course '{request.CourseCode}' already exists for your professor profile (CourseId: {duplicate.Id})"
            });

        var course = new Course
        {
            Name        = request.Name,
            CourseCode  = request.CourseCode,
            ProfessorId = professor.Id
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Course created! CourseId: {course.Id} | Code: {course.CourseCode} | Professor: {professor.FullName}"
        });
    }
}

// ── Request DTOs (local to this controller) ──────────────────────────────────
public class CreateStudentProfileRequest
{
    public string FullName   { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
}

public class CreateProfessorProfileRequest
{
    public string FullName   { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class CreateCourseRequest
{
    public string Name       { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
}
