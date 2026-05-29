using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.API.Repositories;
using Attencial.API.Services;
using Attencial.Shared.Dtos;

namespace Attencial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IFaceService _faceService;
    private readonly IStudentRepository _studentRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<EnrollmentController> _logger;

    public EnrollmentController(
        IFaceService faceService,
        IStudentRepository studentRepo,
        AppDbContext context,
        ILogger<EnrollmentController> logger)
    {
        _faceService = faceService;
        _studentRepo = studentRepo;
        _context = context;
        _logger = logger;
    }

    [HttpGet("status")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetEnrollmentStatus()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<string> { Success = false, Message = "Invalid user identity" });
        }

        var student = await _studentRepo.GetByUserIdAsync(userId);
        if (student is null)
        {
            return NotFound(new ApiResponse<string> { Success = false, Message = "Student profile not found" });
        }

        var latestVector = await _context.FaceVectors
            .Where(fv => fv.StudentId == student.Id)
            .OrderByDescending(fv => fv.CreatedAt)
            .FirstOrDefaultAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new
            {
                EnrollmentStatus = student.EnrollmentStatus,
                IsEnrolled = latestVector != null,
                LastEnrollmentDate = latestVector?.CreatedAt,
                DaysUntilNextUpdate = latestVector != null
                    ? Math.Max(0, Math.Ceiling(7 - (DateTime.UtcNow - latestVector.CreatedAt).TotalDays))
                    : 0
            }
        });
    }

    [HttpPost("detect")]
    public async Task<IActionResult> DetectFace([FromBody] DetectRequest request)
    {
        var result = await _faceService.DetectFaceAsync(request.Image);

        if (result is null)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "No face detected in the image"
            });
        }

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Face detected successfully",
            Data = result
        });
    }

    [HttpPost("enroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollRequest request)
    {
        _logger.LogInformation("EnrollStudent called");

        // Step 1: Validate input
        if (request.Images == null || request.Images.Count != 3)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Exactly 3 face images are required"
            });
        }

        // Step 2: Get the student using the JWT sub (UserId)
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogDebug("JWT UserId claim: {UserIdClaim}", userIdClaim);

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid or missing user identity in JWT token"
            });
        }

        var student = await _studentRepo.GetByUserIdAsync(userId);
        _logger.LogDebug("Student found: {Found}", student != null);

        if (student is null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Student profile not found. Ensure your account has an associated student profile."
            });
        }

        if (string.IsNullOrWhiteSpace(student.RollNumber))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Your student profile has an empty Roll Number. A valid Roll Number is required for face enrollment."
            });
        }

        // Check for existing face vectors to enforce enrollment limits
        var existingVectors = await _context.FaceVectors
            .Where(fv => fv.StudentId == student.Id)
            .OrderByDescending(fv => fv.CreatedAt)
            .ToListAsync();

        if (existingVectors.Any())
        {
            var latest = existingVectors.First();
            var daysSinceLastEnroll = (DateTime.UtcNow - latest.CreatedAt).TotalDays;

            if (daysSinceLastEnroll < 7)
            {
                var waitDays = Math.Ceiling(7 - daysSinceLastEnroll);
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = $"You have already enrolled your face. You are only allowed to update your enrollment once a week. Please wait {waitDays} more day(s)."
                });
            }

            // If 7+ days have passed, delete the old face records from AWS and the database
            _logger.LogInformation("Deleting {Count} old face vectors for student {StudentId}", existingVectors.Count, student.Id);
            foreach (var oldVector in existingVectors)
            {
                try
                {
                    await _faceService.DeleteFaceAsync(oldVector.RekognitionFaceId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old face {FaceId} from AWS", oldVector.RekognitionFaceId);
                }
            }

            _context.FaceVectors.RemoveRange(existingVectors);
        }

        try
        {
            // Step 3: Detect faces in all 3 images (validation)
            _logger.LogInformation("Detecting faces in {Count} images", request.Images.Count);
            var detectedImages = new List<string>();

            for (int i = 0; i < request.Images.Count; i++)
            {
                var detected = await _faceService.DetectFaceAsync(request.Images[i]);
                if (detected is null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = $"Could not detect a face in image {i + 1}. Ensure your face is clearly visible."
                    });
                }
                detectedImages.Add(request.Images[i]);
            }

            // Step 4: Index all 3 faces into Rekognition
            _logger.LogInformation("Indexing faces into Rekognition");
            var rekognitionFaceIds = new List<string>();

            foreach (var image in detectedImages)
            {
                try
                {
                    var faceId = await _faceService.IndexFaceAsync(image, student.RollNumber);
                    rekognitionFaceIds.Add(faceId);
                    _logger.LogDebug("Face indexed with Rekognition FaceId: {FaceId}", faceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Face indexing failed");
                    student.EnrollmentStatus = "Failed";
                    await _studentRepo.UpdateAsync(student);
                    await _studentRepo.SaveChangesAsync();

                    return StatusCode(500, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Face indexing failed. Please try again."
                    });
                }
            }

            // Step 5: Save face records to database
            student.RekognitionExternalId = student.RollNumber;
            student.EnrollmentStatus = "Trained";
            await _studentRepo.UpdateAsync(student);

            foreach (var faceId in rekognitionFaceIds)
            {
                _context.FaceVectors.Add(new FaceVector
                {
                    StudentId             = student.Id,
                    RekognitionFaceId     = faceId,
                    RekognitionExternalId = student.RollNumber
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Enrollment complete for student {StudentId}", student.Id);

            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Enrollment successful! You can now mark attendance with face recognition."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during enrollment");
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = $"Unexpected error during enrollment: {ex.Message}"
            });
        }
    }
}
