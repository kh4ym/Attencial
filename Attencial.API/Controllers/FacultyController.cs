using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.API.Services;
using Attencial.Shared.Dtos;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/faculty")]
public class FacultyController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IFaceService _faceService;

    public FacultyController(AppDbContext context, IFaceService faceService)
    {
        _context = context;
        _faceService = faceService;
    }

    // ── POST /api/faculty/register-face ──────────────────────────────────────
    // Indexes the logged-in professor's face and saves a FaceVector row.
    [HttpPost("register-face")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> RegisterFace([FromBody] FacultyFaceScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Image))
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Image base64 data is required." });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity." });
        }

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });
        }

        var cleanBase64 = request.Image;
        if (cleanBase64.Contains(","))
        {
            cleanBase64 = cleanBase64.Split(',')[1];
        }

        try
        {
            // Index the face in AWS Rekognition
            var externalId = $"prof_{professor.Id}";
            var rekognitionFaceId = await _faceService.IndexFaceAsync(cleanBase64, externalId);

            // Save to database
            var faceVector = new FaceVector
            {
                ProfessorId = professor.Id,
                RekognitionExternalId = externalId,
                RekognitionFaceId = rekognitionFaceId,
                ImageUrl = "Indexed to AWS Rekognition",
                CreatedAt = DateTime.UtcNow
            };

            _context.FaceVectors.Add(faceVector);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Faculty face registered successfully!",
                Data = rekognitionFaceId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = $"Failed to register face: {ex.Message}"
            });
        }
    }

    // ── POST /api/faculty/attendance/checkin ─────────────────────────────────
    // Scans face, identifies professor, and records check-in.
    [HttpPost("attendance/checkin")]
    public async Task<IActionResult> CheckIn([FromBody] FacultyFaceScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Image))
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Image base64 data is required." });
        }

        var cleanBase64 = request.Image;
        if (cleanBase64.Contains(","))
        {
            cleanBase64 = cleanBase64.Split(',')[1];
        }

        try
        {
            // Search face in Rekognition
            var (faceId, similarity) = await _faceService.SearchFaceAsync(cleanBase64);

            if (faceId is null || similarity < 80.0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Face verification failed. We could not match your face with any registered faculty profile."
                });
            }

            // Look up professor
            var faceVector = await _context.FaceVectors
                .Include(fv => fv.Professor)
                .FirstOrDefaultAsync(fv => fv.RekognitionFaceId == faceId && fv.ProfessorId != null);

            if (faceVector is null || faceVector.Professor is null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Face matches a template, but no corresponding faculty profile was found in our database."
                });
            }

            var professor = faceVector.Professor;

            // Check if already checked in (where checkout is null)
            var activeRecord = await _context.FacultyAttendanceRecords
                .FirstOrDefaultAsync(r => r.ProfessorId == professor.Id && r.CheckOutTime == null);

            if (activeRecord is not null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"'{professor.FullName}' is already checked in since {activeRecord.CheckInTime.ToLocalTime():t}."
                });
            }

            // Create check-in record
            var record = new FacultyAttendanceRecord
            {
                ProfessorId = professor.Id,
                CheckInTime = DateTime.UtcNow,
                Status = "CheckedIn"
            };

            _context.FacultyAttendanceRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Check-in successful! Welcome, {professor.FullName}.",
                Data = new
                {
                    ProfessorName = professor.FullName,
                    Department = professor.Department,
                    CheckInTime = record.CheckInTime
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = $"Error during check-in: {ex.Message}"
            });
        }
    }

    // ── POST /api/faculty/attendance/checkout ────────────────────────────────
    // Scans face, identifies professor, and records check-out.
    [HttpPost("attendance/checkout")]
    public async Task<IActionResult> CheckOut([FromBody] FacultyFaceScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Image))
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Image base64 data is required." });
        }

        var cleanBase64 = request.Image;
        if (cleanBase64.Contains(","))
        {
            cleanBase64 = cleanBase64.Split(',')[1];
        }

        try
        {
            // Search face in Rekognition
            var (faceId, similarity) = await _faceService.SearchFaceAsync(cleanBase64);

            if (faceId is null || similarity < 80.0)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Face verification failed. We could not match your face with any registered faculty profile."
                });
            }

            // Look up professor
            var faceVector = await _context.FaceVectors
                .Include(fv => fv.Professor)
                .FirstOrDefaultAsync(fv => fv.RekognitionFaceId == faceId && fv.ProfessorId != null);

            if (faceVector is null || faceVector.Professor is null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Face matches a template, but no corresponding faculty profile was found."
                });
            }

            var professor = faceVector.Professor;

            // Find active check-in record
            var activeRecord = await _context.FacultyAttendanceRecords
                .FirstOrDefaultAsync(r => r.ProfessorId == professor.Id && r.CheckOutTime == null);

            if (activeRecord is null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"No active check-in session found for '{professor.FullName}'. Please check in first."
                });
            }

            // Perform check-out
            var checkOutTime = DateTime.UtcNow;
            var hoursWorked = (checkOutTime - activeRecord.CheckInTime).TotalHours;

            activeRecord.CheckOutTime = checkOutTime;
            activeRecord.HoursWorked = hoursWorked;
            
            // Assign status: ShortShift if < 8 hours, otherwise FullDay
            activeRecord.Status = hoursWorked >= 8.0 ? "FullDay" : "ShortShift";

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Check-out successful! Goodbye, {professor.FullName}.",
                Data = new
                {
                    ProfessorName = professor.FullName,
                    CheckOutTime = activeRecord.CheckOutTime,
                    HoursWorked = activeRecord.HoursWorked,
                    Status = activeRecord.Status
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = $"Error during check-out: {ex.Message}"
            });
        }
    }

    // ── GET /api/admin/faculty/pending ───────────────────────────────────────
    // Admin review queue: returns all pending ShortShift records.
    [HttpGet("/api/admin/faculty/pending")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetPendingShifts()
    {
        var records = await _context.FacultyAttendanceRecords
            .Include(r => r.Professor)
            .Where(r => r.Status == "ShortShift")
            .OrderByDescending(r => r.CheckInTime)
            .Select(r => new
            {
                RecordId = r.Id,
                ProfessorName = r.Professor.FullName,
                Department = r.Professor.Department,
                CheckInTime = r.CheckInTime,
                CheckOutTime = r.CheckOutTime,
                HoursWorked = r.HoursWorked,
                Status = r.Status
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Pending review shifts loaded.",
            Data = records
        });
    }
}
