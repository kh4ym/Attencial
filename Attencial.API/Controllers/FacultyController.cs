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

    [HttpPost("register-face")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> RegisterFace([FromBody] FacultyFaceScanRequest request)
    {
        // Accept both single-image and multi-image formats from the client
        var imageBase64 = request.Image ?? request.Images?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(imageBase64))
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

        var cleanBase64 = imageBase64;
        if (cleanBase64.Contains(","))
        {
            cleanBase64 = cleanBase64.Split(',')[1];
        }

        try
        {
            var externalId = $"prof_{professor.Id}";
            var rekognitionFaceId = await _faceService.IndexFaceAsync(cleanBase64, externalId);

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
}
