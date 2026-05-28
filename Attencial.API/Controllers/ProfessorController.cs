using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/professor")]
public class ProfessorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public ProfessorController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // Helper: Validates JWT token from header or query string, returns UserId if valid
    private int? ValidateUser(out string? errorResponse)
    {
        errorResponse = null;
        var token = Request.Query["token"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring(7).Trim();
            }
        }

        if (string.IsNullOrEmpty(token))
        {
            errorResponse = "Authorization token is missing.";
            return null;
        }

        try
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParameters, out _);
            var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleClaim = principal.FindFirstValue(ClaimTypes.Role);
            if (roleClaim != "Professor")
            {
                errorResponse = "Forbidden: Access denied.";
                return null;
            }
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
        }
        catch (Exception ex)
        {
            errorResponse = $"Invalid token: {ex.Message}";
        }

        return null;
    }

    // ── GET /api/professor/courses/{id:int}/sessions ─────────────────────────
    // Returns sessions list for the course, calculating present and absent numbers.
    [HttpGet("courses/{id:int}/sessions")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetCourseSessions(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && c.ProfessorId == professor.Id);

        if (course is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Course not found or access denied." });

        // Total enrolled student count for absent calculations
        var enrolledCount = await _context.Enrollments
            .CountAsync(e => e.CourseId == id);

        var sessions = await _context.AttendanceSessions
            .Where(s => s.CourseId == id)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var list = new List<ProfessorSessionDto>();
        foreach (var session in sessions)
        {
            var presentCount = await _context.AttendanceRecords
                .CountAsync(ar => ar.SessionId == session.Id);

            var absentCount = Math.Max(0, enrolledCount - presentCount);
            double rate = enrolledCount > 0
                ? (presentCount / (double)enrolledCount) * 100.0
                : 100.0;

            list.Add(new ProfessorSessionDto
            {
                SessionId = session.Id,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                IsActive = session.IsActive,
                PresentCount = presentCount,
                AbsentCount = absentCount,
                AttendanceRate = Math.Round(rate, 1)
            });
        }

        return Ok(new ApiResponse<List<ProfessorSessionDto>>
        {
            Success = true,
            Data = list
        });
    }

    // ── GET /api/professor/courses/{id:int}/abuselogs ────────────────────────
    // Returns all security/abuse log alerts triggered for the course sessions.
    [HttpGet("courses/{id:int}/abuselogs")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetCourseAbuseLogs(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && c.ProfessorId == professor.Id);

        if (course is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Course not found or access denied." });

        var logs = await _context.AbuseLogs
            .Include(al => al.Student)
            .Where(al => al.Session.CourseId == id)
            .OrderByDescending(al => al.CreatedAt)
            .Select(al => new AbuseLogResponseDto
            {
                Id = al.Id,
                SessionId = al.SessionId,
                StudentName = al.Student != null ? al.Student.FullName : "Unknown Student",
                RollNumber = al.Student != null ? al.Student.RollNumber : "N/A",
                AbuseType = al.AbuseType,
                Details = al.Details,
                DeviceId = al.DeviceId,
                IpAddress = al.IpAddress,
                LoggedAt = al.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<List<AbuseLogResponseDto>>
        {
            Success = true,
            Data = logs
        });
    }

    // ── GET /api/professor/courses/{id:int}/export ───────────────────────────
    // Generates a downloadable CSV attendance grid sheet of the course.
    [HttpGet("courses/{id:int}/export")]
    public async Task<IActionResult> ExportCourseAttendance(int id)
    {
        // 1. Authenticate (handles query string for download anchors)
        var userId = ValidateUser(out var authError);
        if (userId is null)
        {
            return Unauthorized(new ApiResponse<string> { Success = false, Message = authError ?? "Unauthorized" });
        }

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId.Value);

        if (professor is null)
            return NotFound("Professor profile not found.");

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && c.ProfessorId == professor.Id);

        if (course is null)
            return NotFound("Course not found or access denied.");

        // 2. Fetch enrolled students
        var students = await _context.Enrollments
            .Include(e => e.Student)
                .ThenInclude(s => s.User)
            .Where(e => e.CourseId == id)
            .Select(e => e.Student)
            .OrderBy(s => s.RollNumber)
            .ToListAsync();

        // 3. Fetch all sessions
        var sessions = await _context.AttendanceSessions
            .Where(s => s.CourseId == id)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // 4. Fetch all attendance records for these sessions
        var records = await _context.AttendanceRecords
            .Where(ar => ar.Session.CourseId == id)
            .ToListAsync();

        // 5. Generate CSV Content
        var sb = new StringBuilder();

        // Header Row
        sb.Append("Student Name,Roll Number,Email");
        foreach (var session in sessions)
        {
            sb.Append($",Session #{session.Id} ({session.StartTime.ToLocalTime():yyyy-MM-dd})");
        }
        sb.AppendLine(",Present Count,Total Sessions,Attendance Rate (%)");

        // Data Rows
        foreach (var student in students)
        {
            sb.Append($"\"{student.FullName}\",\"{student.RollNumber}\",\"{student.User?.Email ?? "N/A"}\"");
            int presentCount = 0;
            foreach (var session in sessions)
            {
                bool isPresent = records.Any(r => r.SessionId == session.Id && r.StudentId == student.Id);
                if (isPresent)
                {
                    sb.Append(",P");
                    presentCount++;
                }
                else
                {
                    sb.Append(",A");
                }
            }

            double rate = sessions.Count > 0
                ? (presentCount / (double)sessions.Count) * 100.0
                : 100.0;

            sb.AppendLine($",{presentCount},{sessions.Count},{Math.Round(rate, 1)}");
        }

        var csvString = sb.ToString();
        var bytes = Encoding.UTF8.GetBytes(csvString);
        var contentDisposition = $"attachment; filename=Attendance_Report_{course.CourseCode.Replace(" ", "_")}.csv";

        Response.Headers.Append("Content-Disposition", contentDisposition);
        return File(bytes, "text/csv");
    }

    [HttpGet("appeals/pending")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetPendingAppeals()
    {
        var appeals = await _context.AttendanceAppeals
            .Include(a => a.Student)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                StudentName = a.Student.FullName,
                RollNumber = a.Student.RollNumber,
                a.CourseName,
                a.Reason,
                a.Status,
                SessionDate = a.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = appeals });
    }

    [HttpPut("appeals/{id:int}/approve")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> ApproveAppeal(int id)
    {
        var appeal = await _context.AttendanceAppeals.FindAsync(id);
        if (appeal is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Appeal not found." });

        appeal.Status = "Approved";

        // Auto-mark attendance for the appealed session
        var alreadyMarked = await _context.AttendanceRecords
            .AnyAsync(ar => ar.SessionId == appeal.SessionId && ar.StudentId == appeal.StudentId);
        if (!alreadyMarked)
        {
            _context.AttendanceRecords.Add(new AttendanceRecord
            {
                SessionId = appeal.SessionId,
                StudentId = appeal.StudentId,
                Confidence = 100f,
                DeviceId = "appeal-approved",
                MarkedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<string> { Success = true, Message = "Appeal approved." });
    }

    [HttpPut("appeals/{id:int}/reject")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> RejectAppeal(int id)
    {
        var appeal = await _context.AttendanceAppeals.FindAsync(id);
        if (appeal is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Appeal not found." });

        appeal.Status = "Rejected";
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<string> { Success = true, Message = "Appeal rejected." });
    }
}
