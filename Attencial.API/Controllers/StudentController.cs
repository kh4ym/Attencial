using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Constants;
using Attencial.Shared.Dtos;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("me/attendance")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<IActionResult> GetMyAttendance()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity." });
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Student profile not found." });
        }

        // Single query: get all enrollments with courses and professors
        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
                .ThenInclude(c => c.Professor)
            .Where(e => e.StudentId == student.Id)
            .ToListAsync();

        if (enrollments.Count == 0)
        {
            return Ok(new ApiResponse<StudentAttendanceSummaryDto>
            {
                Success = true,
                Data = new StudentAttendanceSummaryDto
                {
                    OverallPercentage = 100.0,
                    CourseAttendance = new List<StudentCourseAttendanceDto>()
                }
            });
        }

        var courseIds = enrollments.Select(e => e.CourseId).ToList();

        // Single query: all sessions for all enrolled courses
        var sessions = await _context.AttendanceSessions
            .AsNoTracking()
            .Where(s => courseIds.Contains(s.CourseId))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        var appealStatuses = await _context.AttendanceAppeals
            .AsNoTracking()
            .Where(a => a.StudentId == student.Id)
            .Select(a => new { a.SessionId, a.Status })
            .ToListAsync();
        var appealBySessionId = appealStatuses.ToDictionary(a => a.SessionId, a => a.Status);

        // Single query: all attendance records for this student across all enrolled courses
        var attendedSessionIds = (await _context.AttendanceRecords
            .AsNoTracking()
            .Where(ar => ar.StudentId == student.Id)
            .Select(ar => ar.SessionId)
            .ToListAsync())
            .ToHashSet();

        var courseAttendanceList = new List<StudentCourseAttendanceDto>();
        int totalSessionsAll = 0;
        int totalAttendedAll = 0;

        foreach (var enrollment in enrollments)
        {
            var course = enrollment.Course;
            var courseSessions = sessions.Where(s => s.CourseId == course.Id).ToList();

            int totalSessions = courseSessions.Count;
            int attendedSessions = courseSessions.Count(s => attendedSessionIds.Contains(s.Id));

            totalSessionsAll += totalSessions;
            totalAttendedAll += attendedSessions;

            double percentage = totalSessions > 0
                ? (attendedSessions / (double)totalSessions) * 100.0
                : 100.0;

            string status = AppConstants.AttendanceStatuses.Green;
            if (percentage < 65.0)
                status = AppConstants.AttendanceStatuses.Red;
            else if (percentage < 75.0)
                status = AppConstants.AttendanceStatuses.Yellow;

            var allSessions = courseSessions
                .OrderByDescending(s => s.StartTime)
                .Select(s => new AttendanceSessionDto
                {
                    SessionId = s.Id,
                    Date = s.StartTime,
                    IsPresent = attendedSessionIds.Contains(s.Id),
                    AppealStatus = appealBySessionId.TryGetValue(s.Id, out var appealStatus) ? appealStatus : null
                })
                .ToList();

            courseAttendanceList.Add(new StudentCourseAttendanceDto
            {
                CourseId = course.Id,
                CourseName = course.Name,
                CourseCode = course.CourseCode,
                ProfessorName = course.Professor?.FullName ?? "Unknown Professor",
                TotalSessions = totalSessions,
                AttendedSessions = attendedSessions,
                Percentage = Math.Round(percentage, 1),
                Status = status,
                Sessions = allSessions
            });
        }

        double overallPercentage = totalSessionsAll > 0
            ? (totalAttendedAll / (double)totalSessionsAll) * 100.0
            : 100.0;

        var summary = new StudentAttendanceSummaryDto
        {
            OverallPercentage = Math.Round(overallPercentage, 1),
            TotalCourses = enrollments.Count,
            PresentSessions = totalAttendedAll,
            TotalSessions = totalSessionsAll,
            CourseAttendance = courseAttendanceList
        };

        return Ok(new ApiResponse<StudentAttendanceSummaryDto>
        {
            Success = true,
            Data = summary
        });
    }

    [HttpGet("me/appeals")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<IActionResult> GetMyAppeals()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Student profile not found." });

        var appeals = await _context.AttendanceAppeals
            .AsNoTracking()
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.SessionId,
                a.CourseName,
                a.Reason,
                a.Status,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = appeals });
    }

    [HttpPost("me/appeal")]
    [Authorize(Roles = AppConstants.Roles.Student)]
    public async Task<IActionResult> SubmitAppeal([FromBody] SubmitAppealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Reason is required." });

        if (request.Reason.Trim().Length > 1000)
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Reason must be 1000 characters or fewer." });

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Student profile not found." });

        var session = await _context.AttendanceSessions
            .AsNoTracking()
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if (session is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Attendance session not found." });

        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == student.Id && e.CourseId == session.CourseId);

        if (!isEnrolled)
            return StatusCode(403, new ApiResponse<string> { Success = false, Message = "You can only appeal sessions for courses you are enrolled in." });

        var alreadyMarked = await _context.AttendanceRecords
            .AnyAsync(ar => ar.StudentId == student.Id && ar.SessionId == session.Id);

        if (alreadyMarked)
            return Conflict(new ApiResponse<string> { Success = false, Message = "You are already marked present for this session." });

        var existing = await _context.AttendanceAppeals
            .AnyAsync(a => a.StudentId == student.Id && a.SessionId == request.SessionId);
        if (existing)
            return Conflict(new ApiResponse<string> { Success = false, Message = "You have already submitted an appeal for this session." });

        var appeal = new AttendanceAppeal
        {
            StudentId = student.Id,
            SessionId = request.SessionId,
            CourseName = session.Course.Name,
            Reason = request.Reason.Trim()
        };

        _context.AttendanceAppeals.Add(appeal);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string> { Success = true, Message = "Appeal submitted successfully." });
    }
}

public class SubmitAppealRequest
{
    public int SessionId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
