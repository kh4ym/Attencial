using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Dtos;
using FluentValidation;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IValidator<LeaveRequestCreateRequest> _createValidator;
    private readonly IValidator<LeaveRequestReviewRequest> _reviewValidator;

    public LeaveController(
        AppDbContext context, 
        IWebHostEnvironment env,
        IValidator<LeaveRequestCreateRequest> createValidator,
        IValidator<LeaveRequestReviewRequest> reviewValidator)
    {
        _context = context;
        _env = env;
        _createValidator = createValidator;
        _reviewValidator = reviewValidator;
    }

    // ── POST /api/leave ──────────────────────────────────────────────────────
    // Submits a new leave request. Saves PDF attachment locally if provided.
    [HttpPost]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestCreateRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
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

        string? attachmentUrl = null;

        if (!string.IsNullOrEmpty(request.AttachmentBase64))
        {
            try
            {
                var base64Data = request.AttachmentBase64;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                var fileBytes = Convert.FromBase64String(base64Data);

                // Setup uploads directory inside wwwroot
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsDir = Path.Combine(webRoot, "uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var safeFileName = request.AttachmentFileName ?? "attachment.pdf";
                // Replace invalid filename characters
                foreach (var c in Path.GetInvalidFileNameChars())
                {
                    safeFileName = safeFileName.Replace(c, '_');
                }

                var fileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(uploadsDir, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
                attachmentUrl = $"/uploads/{fileName}";
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Failed to process file attachment: {ex.Message}"
                });
            }
        }

        var leave = new LeaveRequest
        {
            ProfessorId = professor.Id,
            LeaveType = request.LeaveType,
            Reason = request.Reason,
            StartDate = request.StartDate.ToUniversalTime(),
            EndDate = request.EndDate.ToUniversalTime(),
            AttachmentUrl = attachmentUrl,
            Status = "Pending"
        };

        _context.LeaveRequests.Add(leave);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Leave request submitted successfully!",
            Data = leave.Id.ToString()
        });
    }

    // ── GET /api/leave ───────────────────────────────────────────────────────
    // Returns leave requests submitted by the logged-in professor.
    [HttpGet]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetMyLeaveRequests()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (professor is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Professor profile not found." });
        }

        var list = await _context.LeaveRequests
            .Where(l => l.ProfessorId == professor.Id)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveRequestResponseDto
            {
                Id = l.Id,
                ProfessorName = professor.FullName,
                Department = professor.Department,
                LeaveType = l.LeaveType,
                Reason = l.Reason,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                AttachmentUrl = l.AttachmentUrl,
                Status = l.Status,
                AdminNote = l.AdminNote,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = list
        });
    }

    // ── GET /api/admin/leave/pending ─────────────────────────────────────────
    // Returns all pending leave requests for admin review.
    [HttpGet("/api/admin/leave/pending")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> GetPendingLeaveRequests()
    {
        var list = await _context.LeaveRequests
            .Include(l => l.Professor)
            .Where(l => l.Status == "Pending")
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveRequestResponseDto
            {
                Id = l.Id,
                ProfessorName = l.Professor.FullName,
                Department = l.Professor.Department,
                LeaveType = l.LeaveType,
                Reason = l.Reason,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                AttachmentUrl = l.AttachmentUrl,
                Status = l.Status,
                AdminNote = l.AdminNote,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = list
        });
    }

    // ── PUT /api/admin/leave/{id}/approve ────────────────────────────────────
    // Approves a pending leave request with a note (min 10 chars).
    [HttpPut("/api/admin/leave/{id:int}/approve")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> ApproveLeaveRequest(int id, [FromBody] LeaveRequestReviewRequest request)
    {
        var validationResult = await _reviewValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var leave = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leave is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Leave request not found." });
        }

        if (leave.Status != "Pending")
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Only pending leave requests can be reviewed." });
        }

        leave.Status = "Approved";
        leave.AdminNote = request.AdminNote.Trim();

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Leave request approved successfully!"
        });
    }

    // ── PUT /api/admin/leave/{id}/reject ────────────────────────────────────
    // Rejects a pending leave request with a note (min 10 chars).
    [HttpPut("/api/admin/leave/{id:int}/reject")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> RejectLeaveRequest(int id, [FromBody] LeaveRequestReviewRequest request)
    {
        var validationResult = await _reviewValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var leave = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leave is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Leave request not found." });
        }

        if (leave.Status != "Pending")
        {
            return BadRequest(new ApiResponse<string> { Success = false, Message = "Only pending leave requests can be reviewed." });
        }

        leave.Status = "Rejected";
        leave.AdminNote = request.AdminNote.Trim();

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Leave request rejected successfully!"
        });
    }
}
