using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.API.Services;
using Attencial.Shared.Dtos;
using FluentValidation;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/attendance")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IAttendanceService _attendanceService;
    private readonly IValidator<CreateSessionRequest> _sessionValidator;
    private readonly IValidator<AttendanceMarkRequest> _markValidator;

    public AttendanceController(
        AppDbContext context, 
        IConfiguration config, 
        IAttendanceService attendanceService,
        IValidator<CreateSessionRequest> sessionValidator,
        IValidator<AttendanceMarkRequest> markValidator)
    {
        _context = context;
        _config = config;
        _attendanceService = attendanceService;
        _sessionValidator = sessionValidator;
        _markValidator = markValidator;
    }

    // ── POST /api/attendance/sessions ────────────────────────────────────────
    // Professor creates a new attendance session for one of their courses.
    // Returns the session + a 64-char crypto-random token + the attendance URL.
    [HttpPost("sessions")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var validationResult = await _sessionValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Step 2: Resolve the Professor from the JWT UserId
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid token." });

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Professor profile not found. Make sure your account has a professor profile."
            });

        // Step 3: Verify the course belongs to this professor
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == request.CourseId && c.ProfessorId == professor.Id);

        if (course is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Course not found or you are not the professor of this course."
            });

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Step 4: Deactivate any currently active session for this course
        // (only one active session per course at a time)
        var activeSessions = await _context.AttendanceSessions
            .Include(s => s.Token)
            .Where(s => s.CourseId == course.Id && s.IsActive)
            .ToListAsync();

        foreach (var old in activeSessions)
        {
            old.IsActive = false;
            old.EndTime = DateTime.UtcNow;
            if (old.Token is not null)
                old.Token.IsActive = false;
        }

        // Step 5: Generate a cryptographically secure 64-char token
        var tokenBytes = RandomNumberGenerator.GetBytes(48); // 48 bytes -> 64 Base64 chars
        var tokenString = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            [..64];

        // Step 6: Create the AttendanceSession row
        var session = new AttendanceSession
        {
            CourseId    = course.Id,
            ProfessorId = professor.Id,
            StartTime   = DateTime.UtcNow,
            IsActive    = true
        };
        _context.AttendanceSessions.Add(session);
        await _context.SaveChangesAsync(); // Need the SessionId before creating Token

        // Step 7: Create the OnlineAttendanceToken row
        var expiresAt = DateTime.UtcNow.AddMinutes(request.ExpiryMinutes);
        var token = new OnlineAttendanceToken
        {
            SessionId     = session.Id,
            Token         = tokenString,
            ExpiryMinutes = request.ExpiryMinutes,
            ExpiresAt     = expiresAt,
            IsActive      = true
        };
        _context.OnlineAttendanceTokens.Add(token);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        // Step 8: Build the attendance URL that students will scan
        var clientBaseUrl = _config["ClientBaseUrl"] ?? "http://localhost:7251";
        var attendanceUrl = $"{clientBaseUrl}/attend?token={tokenString}";

        return Ok(new ApiResponse<SessionResponseDto>
        {
            Success = true,
            Message = "Session started successfully.",
            Data = new SessionResponseDto
            {
                SessionId     = session.Id,
                CourseId      = course.Id,
                CourseName    = course.Name,
                CourseCode    = course.CourseCode,
                Token         = tokenString,
                ExpiryMinutes = request.ExpiryMinutes,
                ExpiresAt     = expiresAt,
                IsActive      = true,
                CreatedAt     = session.CreatedAt,
                AttendanceUrl = attendanceUrl
            }
        });
    }

    // ── GET /api/attendance/sessions/{id} ────────────────────────────────────
    // Lets the professor page poll the session to check if it's still active
    // and get an updated expiry time (for the countdown timer).
    [HttpGet("sessions/{id:int}")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetSession(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound();

        var session = await _context.AttendanceSessions
            .Include(s => s.Course)
            .Include(s => s.Token)
            .FirstOrDefaultAsync(s => s.Id == id && s.ProfessorId == professor.Id);

        if (session is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Session not found." });

        // Auto-expire: if token time has passed, mark session inactive
        if (session.Token is not null && session.Token.ExpiresAt < DateTime.UtcNow && session.IsActive)
        {
            session.IsActive = false;
            session.EndTime  = DateTime.UtcNow;
            session.Token.IsActive = false;
            await _context.SaveChangesAsync();
        }

        var clientBaseUrl = _config["ClientBaseUrl"] ?? "http://localhost:7251";
        var attendanceUrl = session.Token is not null
            ? $"{clientBaseUrl}/attend?token={session.Token.Token}"
            : string.Empty;

        return Ok(new ApiResponse<SessionResponseDto>
        {
            Success = true,
            Data = new SessionResponseDto
            {
                SessionId     = session.Id,
                CourseId      = session.CourseId,
                CourseName    = session.Course.Name,
                CourseCode    = session.Course.CourseCode,
                Token         = session.Token?.Token ?? string.Empty,
                ExpiryMinutes = session.Token?.ExpiryMinutes ?? 0,
                ExpiresAt     = session.Token?.ExpiresAt ?? DateTime.UtcNow,
                IsActive      = session.IsActive,
                CreatedAt     = session.CreatedAt,
                AttendanceUrl = attendanceUrl
            }
        });
    }

    // ── DELETE /api/attendance/sessions/{id}/end ──────────────────────────────
    // Professor manually ends a session before its token expires.
    [HttpDelete("sessions/{id:int}/end")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> EndSession(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound();

        var session = await _context.AttendanceSessions
            .Include(s => s.Token)
            .FirstOrDefaultAsync(s => s.Id == id && s.ProfessorId == professor.Id);

        if (session is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Session not found." });

        session.IsActive = false;
        session.EndTime  = DateTime.UtcNow;

        if (session.Token is not null)
            session.Token.IsActive = false;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Session ended successfully."
        });
    }

    // ── GET /api/attendance/professor/courses ─────────────────────────────────
    // Returns all courses belonging to the logged-in professor (for the dropdown).
    [HttpGet("professor/courses")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetProfessorCourses()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Professor profile not found."
            });

        var courses = await _context.Courses
            .Where(c => c.ProfessorId == professor.Id)
            .Select(c => new { c.Id, c.Name, c.CourseCode })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data    = courses
        });
    }

    // ── GET /api/attendance/sessions/active ──────────────────────────────────
    // Returns the professor's currently active session (if any), so it survives
    // page navigation and tab switches in the frontend.
    [HttpGet("sessions/active")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetActiveSession()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound();

        var session = await _context.AttendanceSessions
            .Include(s => s.Course)
            .Include(s => s.Token)
            .FirstOrDefaultAsync(s => s.ProfessorId == professor.Id && s.IsActive);

        if (session is null || session.Token is null)
            return Ok(new ApiResponse<object> { Success = true, Data = null });

        var clientBaseUrl = _config["ClientBaseUrl"] ?? "http://localhost:7251";
        var attendanceUrl = $"{clientBaseUrl}/attend?token={session.Token.Token}";

        return Ok(new ApiResponse<SessionResponseDto>
        {
            Success = true,
            Data = new SessionResponseDto
            {
                SessionId     = session.Id,
                CourseId      = session.CourseId,
                CourseName    = session.Course.Name,
                CourseCode    = session.Course.CourseCode,
                Token         = session.Token.Token,
                ExpiryMinutes = session.Token.ExpiryMinutes,
                ExpiresAt     = session.Token.ExpiresAt,
                IsActive      = session.IsActive && session.Token.ExpiresAt > DateTime.UtcNow,
                CreatedAt     = session.CreatedAt,
                AttendanceUrl = attendanceUrl
            }
        });
    }

    // ── GET /api/attendance/sessions/validate ────────────────────────────────
    // Validates if an attendance token is active and not expired.
    [HttpGet("sessions/validate")]
    public async Task<IActionResult> ValidateToken([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Token parameter is required." });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _attendanceService.ValidateTokenAsync(token, ipAddress);
        
        if (!result.Success)
        {
            if (result.Message.Contains("Rate limit exceeded"))
                return StatusCode(429, result);
            return BadRequest(result);
        }

        return Ok(result);
    }

    // ── POST /api/attendance/mark ────────────────────────────────────────────
    // Marks student attendance after executing the 8-layered pipeline.
    [HttpPost("mark")]
    public async Task<IActionResult> MarkAttendance([FromBody] AttendanceMarkRequest request)
    {
        var validationResult = await _markValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var result = await _attendanceService.MarkAttendanceAsync(request, ipAddress);

        if (!result.Success)
        {
            if (result.Message.Contains("Rate limit exceeded"))
                return StatusCode(429, result);
            
            if (result.Message.Contains("Access Denied"))
                return StatusCode(403, result);

            return BadRequest(result);
        }

        return Ok(result);
    }

    // ── GET /api/attendance/courses/{courseId:int}/enrolled-students ──────────
    [HttpGet("courses/{courseId:int}/enrolled-students")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetEnrolledStudents(int courseId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var ownsCourse = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.ProfessorId == professor.Id);

        if (!ownsCourse)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Course not found or access denied." });

        var list = await _context.Enrollments
            .Include(e => e.Student)
            .Where(e => e.CourseId == courseId)
            .Select(e => new
            {
                e.Student.Id,
                e.Student.FullName,
                e.Student.RollNumber
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = list
        });
    }

    // ── GET /api/attendance/config/supabase-realtime ──────────────────────────
    [HttpGet("config/supabase-realtime")]
    [Authorize(Roles = "Professor")]
    public IActionResult GetSupabaseConfig()
    {
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new
            {
                Url = _config["Supabase:Url"] ?? "https://cgvytnlvjjkibneltthx.supabase.co",
                AnonKey = _config["Supabase:AnonKey"] ?? "placeholder"
            }
        });
    }

    // ── GET /api/attendance/sessions/{id:int}/records ─────────────────────────
    [HttpGet("sessions/{id:int}/records")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetSessionRecords(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var ownsSession = await _context.AttendanceSessions
            .AnyAsync(s => s.Id == id && s.ProfessorId == professor.Id);

        if (!ownsSession)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Session not found or access denied." });

        var records = await _context.AttendanceRecords
            .Include(ar => ar.Student)
            .Where(ar => ar.SessionId == id)
            .OrderByDescending(ar => ar.MarkedAt)
            .Select(ar => new
            {
                StudentId = ar.StudentId,
                StudentName = ar.Student.FullName,
                RollNumber = ar.Student.RollNumber,
                Confidence = ar.Confidence,
                MarkedAt = ar.MarkedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = records
        });
    }


    // ── DELETE /api/attendance/sessions/{id:int} ──────────────────────────────
    // Professor manually deletes a session and its associated records/token/appeals.
    [HttpDelete("sessions/{id:int}")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> DeleteSessionRecord(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity." });

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var session = await _context.AttendanceSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.ProfessorId == professor.Id);

        if (session is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Session not found or access denied." });

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var records = await _context.AttendanceRecords.Where(ar => ar.SessionId == id).ToListAsync();
            _context.AttendanceRecords.RemoveRange(records);

            var tokens = await _context.OnlineAttendanceTokens.Where(t => t.SessionId == id).ToListAsync();
            _context.OnlineAttendanceTokens.RemoveRange(tokens);

            var appeals = await _context.AttendanceAppeals.Where(a => a.SessionId == id).ToListAsync();
            _context.AttendanceAppeals.RemoveRange(appeals);

            _context.AttendanceSessions.Remove(session);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new ApiResponse<string> { Success = true, Message = "Session deleted successfully." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new ApiResponse<string> { Success = false, Message = $"Error deleting session: {ex.Message}" });
        }
    }

    // ── PUT /api/attendance/sessions/{id:int}/records ──────────────────────────
    // Manual override of student attendance list for a session.
    [HttpPut("sessions/{id:int}/records")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> UpdateSessionRecords(int id, [FromBody] UpdateSessionRecordsRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity." });

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });

        var session = await _context.AttendanceSessions
            .FirstOrDefaultAsync(s => s.Id == id && s.ProfessorId == professor.Id);

        if (session is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Session not found or access denied." });

        var existingRecords = await _context.AttendanceRecords
            .Where(ar => ar.SessionId == id)
            .ToListAsync();

        var existingStudentIds = existingRecords.Select(ar => ar.StudentId).ToHashSet();
        var targetStudentIds = (request.PresentStudentIds ?? new List<int>()).ToHashSet();

        // Add missing records
        var toAdd = targetStudentIds.Except(existingStudentIds);
        foreach (var studentId in toAdd)
        {
            _context.AttendanceRecords.Add(new AttendanceRecord
            {
                SessionId = id,
                StudentId = studentId,
                Confidence = 100f,
                DeviceId = "manual-override",
                MarkedAt = DateTime.UtcNow
            });
        }

        // Remove records that are not in target list
        var toRemove = existingRecords.Where(ar => !targetStudentIds.Contains(ar.StudentId));
        _context.AttendanceRecords.RemoveRange(toRemove);

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Success = true, Message = "Attendance records updated successfully." });
    }
}

public class UpdateSessionRecordsRequest
{
    public List<int> PresentStudentIds { get; set; } = new();
}
