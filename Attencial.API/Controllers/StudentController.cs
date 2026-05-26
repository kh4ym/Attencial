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

    // ── GET /api/students/me/attendance ──────────────────────────────────────
    // Calculates overall and per-course attendance and lists missed sessions.
    [HttpGet("me/attendance")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAttendance()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity." });
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Student profile not found." });
        }

        // Get enrollments for the student
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Professor)
            .Where(e => e.StudentId == student.Id)
            .ToListAsync();

        var courseAttendanceList = new List<StudentCourseAttendanceDto>();
        int totalSessionsAll = 0;
        int totalAttendedAll = 0;

        foreach (var enrollment in enrollments)
        {
            var course = enrollment.Course;

            // Get total sessions for this course
            var sessions = await _context.AttendanceSessions
                .Where(s => s.CourseId == course.Id)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            // Get attended session records
            var records = await _context.AttendanceRecords
                .Where(ar => ar.StudentId == student.Id && ar.Session.CourseId == course.Id)
                .Select(ar => ar.SessionId)
                .ToListAsync();

            int totalSessions = sessions.Count;
            int attendedSessions = records.Count;

            totalSessionsAll += totalSessions;
            totalAttendedAll += attendedSessions;

            double percentage = totalSessions > 0
                ? (attendedSessions / (double)totalSessions) * 100.0
                : 100.0;

            string status = "Green";
            if (percentage < 65.0)
            {
                status = "Red";
            }
            else if (percentage < 75.0)
            {
                status = "Yellow";
            }

            // Find missed sessions
            var missedSessions = sessions
                .Where(s => !records.Contains(s.Id))
                .Select(s => new MissedSessionDto
                {
                    SessionId = s.Id,
                    Date = s.StartTime
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
                MissedSessions = missedSessions
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
}
