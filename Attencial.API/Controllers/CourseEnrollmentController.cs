using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseEnrollmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public CourseEnrollmentController(AppDbContext context)
    {
        _context = context;
    }

    // ── GET /api/courses ─────────────────────────────────────────────────────
    // Returns all courses with the requesting student's own enrollment status.
    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetCourses()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Student profile not found. Please create your profile first via /api/seed/create-student-profile."
            });

        // Load all courses with professor info
        var courses = await _context.Courses
            .Include(c => c.Professor)
            .OrderBy(c => c.Name)
            .ToListAsync();

        // Load this student's enrollment requests and actual enrollments
        var requests = await _context.EnrollmentRequests
            .Where(er => er.StudentId == student.Id)
            .ToListAsync();

        var enrolledCourseIds = (await _context.Enrollments
            .Where(e => e.StudentId == student.Id)
            .Select(e => e.CourseId)
            .ToListAsync())
            .ToHashSet();

        var dtos = courses.Select(c =>
        {
            // Approved requests will also have an Enrollment row — prioritise that
            if (enrolledCourseIds.Contains(c.Id))
                return new CourseDto
                {
                    Id                      = c.Id,
                    Name                    = c.Name,
                    CourseCode              = c.CourseCode,
                    ProfessorName           = c.Professor.FullName,
                    Department              = c.Professor.Department,
                    EnrollmentRequestStatus = "Approved"
                };

            var req = requests.FirstOrDefault(r => r.CourseId == c.Id);
            return new CourseDto
            {
                Id                      = c.Id,
                Name                    = c.Name,
                CourseCode              = c.CourseCode,
                ProfessorName           = c.Professor.FullName,
                Department              = c.Professor.Department,
                EnrollmentRequestStatus = req?.Status ?? "None",
                Note                    = req?.Note
            };
        }).ToList();

        return Ok(new ApiResponse<List<CourseDto>> { Success = true, Data = dtos });
    }

    // ── POST /api/courses/{courseId}/enrollment-requests ─────────────────────
    // Student submits a request to join a course.
    [HttpPost("{courseId:int}/enrollment-requests")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> RequestEnrollment(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

        if (student is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Student profile not found."
            });

        var course = await _context.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound(new ApiResponse<string> { Success = false, Message = "Course not found." });

        // Already formally enrolled?
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == student.Id && e.CourseId == courseId);
        if (alreadyEnrolled)
            return Conflict(new ApiResponse<string>
            {
                Success = false,
                Message = "You are already enrolled in this course."
            });

        // Existing request?
        var existing = await _context.EnrollmentRequests
            .FirstOrDefaultAsync(er => er.StudentId == student.Id && er.CourseId == courseId);

        if (existing is not null)
        {
            if (existing.Status == "Pending")
                return Conflict(new ApiResponse<string>
                {
                    Success = false,
                    Message = "You already have a pending enrollment request for this course."
                });

            if (existing.Status == "Approved")
                return Conflict(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Your enrollment request for this course was already approved."
                });

            // Status == "Rejected" → allow re-request by resetting the existing row
            existing.Status      = "Pending";
            existing.Note        = null;
            existing.RequestedAt = DateTime.UtcNow;
            existing.ReviewedAt  = null;
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = $"Re-enrollment request submitted for '{course.Name}'. Awaiting professor approval."
            });
        }

        var request = new EnrollmentRequest
        {
            StudentId = student.Id,
            CourseId  = courseId,
            Status    = "Pending"
        };

        _context.EnrollmentRequests.Add(request);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Enrollment request submitted for '{course.Name}'. Awaiting professor approval."
        });
    }

    // ── GET /api/courses/enrollment-requests/pending ──────────────────────────
    // Professor: all pending enrollment requests for courses they own.
    [HttpGet("enrollment-requests/pending")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Professor profile not found."
            });

        var requests = await _context.EnrollmentRequests
            .Include(er => er.Student)
            .Include(er => er.Course)
            .Where(er => er.Course.ProfessorId == professor.Id && er.Status == "Pending")
            .OrderBy(er => er.RequestedAt)
            .Select(er => new EnrollmentRequestDto
            {
                Id          = er.Id,
                StudentId   = er.StudentId,
                StudentName = er.Student.FullName,
                RollNumber  = er.Student.RollNumber,
                CourseId    = er.CourseId,
                CourseName  = er.Course.Name,
                CourseCode  = er.Course.CourseCode,
                Status      = er.Status,
                Note        = er.Note,
                RequestedAt = er.RequestedAt,
                ReviewedAt  = er.ReviewedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<List<EnrollmentRequestDto>> { Success = true, Data = requests });
    }

    // ── PUT /api/courses/enrollment-requests/{requestId}/approve ──────────────
    // Professor approves the request → inserts an Enrollment row.
    [HttpPut("enrollment-requests/{requestId:int}/approve")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> ApproveRequest(int requestId)
    {
        var (professor, request, error) = await ResolveRequest(requestId);
        if (error is not null) return error;

        // Insert the real Enrollment record
        var alreadyEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == request!.StudentId && e.CourseId == request.CourseId);

        if (!alreadyEnrolled)
        {
            _context.Enrollments.Add(new Enrollment
            {
                StudentId = request!.StudentId,
                CourseId  = request.CourseId
            });
        }

        request!.Status     = "Approved";
        request.ReviewedAt  = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Enrollment approved. {request.Student.FullName} is now enrolled in '{request.Course.Name}'."
        });
    }

    // ── PUT /api/courses/enrollment-requests/{requestId}/reject ──────────────
    // Professor rejects the request with an optional note.
    [HttpPut("enrollment-requests/{requestId:int}/reject")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> RejectRequest(int requestId, [FromBody] RejectEnrollmentRequest body)
    {
        var (professor, request, error) = await ResolveRequest(requestId);
        if (error is not null) return error;

        request!.Status    = "Rejected";
        request.Note       = body.Note?.Trim();
        request.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = $"Enrollment request from {request.Student.FullName} for '{request.Course.Name}' has been rejected."
        });
    }

    // ── Helper ────────────────────────────────────────────────────────────────
    private async Task<(Professor? professor, EnrollmentRequest? request, IActionResult? error)>
        ResolveRequest(int requestId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var professor = await _context.Professors.FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
            return (null, null, NotFound(new ApiResponse<string>
            {
                Success = false, Message = "Professor profile not found."
            }));

        var request = await _context.EnrollmentRequests
            .Include(er => er.Student)
            .Include(er => er.Course)
            .FirstOrDefaultAsync(er => er.Id == requestId);

        if (request is null)
            return (professor, null, NotFound(new ApiResponse<string>
            {
                Success = false, Message = "Enrollment request not found."
            }));

        if (request.Course.ProfessorId != professor.Id)
            return (professor, null, Forbid());

        if (request.Status != "Pending")
            return (professor, null, BadRequest(new ApiResponse<string>
            {
                Success = false, Message = $"This request has already been {request.Status.ToLower()}."
            }));

        return (professor, request, null);
    }

    // ── DELETE /api/courses/{id:int} ──────────────────────────────────────────
    // Professor: deletes a course completely, cascading deletes to all associated data.
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> DeleteCourse(int id)
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

        // Manually delete dependent records to avoid DB constraint failures
        var sessionIds = await _context.AttendanceSessions
            .Where(s => s.CourseId == id)
            .Select(s => s.Id)
            .ToListAsync();

        if (sessionIds.Any())
        {
            var records = _context.AttendanceRecords.Where(r => sessionIds.Contains(r.SessionId));
            _context.AttendanceRecords.RemoveRange(records);

            var tokens = _context.OnlineAttendanceTokens.Where(t => sessionIds.Contains(t.SessionId));
            _context.OnlineAttendanceTokens.RemoveRange(tokens);

            var appeals = _context.AttendanceAppeals.Where(a => sessionIds.Contains(a.SessionId));
            _context.AttendanceAppeals.RemoveRange(appeals);

            var abuseLogs = _context.AbuseLogs.Where(al => al.SessionId != null && sessionIds.Contains(al.SessionId.Value));
            _context.AbuseLogs.RemoveRange(abuseLogs);

            var sessions = _context.AttendanceSessions.Where(s => s.CourseId == id);
            _context.AttendanceSessions.RemoveRange(sessions);
        }

        var enrollments = _context.Enrollments.Where(e => e.CourseId == id);
        _context.Enrollments.RemoveRange(enrollments);

        var enrollmentRequests = _context.EnrollmentRequests.Where(er => er.CourseId == id);
        _context.EnrollmentRequests.RemoveRange(enrollmentRequests);

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Course and all associated data deleted successfully."
        });
    }
}

// ── Request DTO ───────────────────────────────────────────────────────────────
public class RejectEnrollmentRequest
{
    public string? Note { get; set; }
}
