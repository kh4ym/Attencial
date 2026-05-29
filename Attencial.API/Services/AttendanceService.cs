using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Attencial.API.Data;
using Attencial.API.Models;
using Attencial.Shared.Constants;
using Attencial.Shared.Dtos;

namespace Attencial.API.Services;

public class AttendanceService : IAttendanceService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> CacheKeyLocks = new();

    private readonly AppDbContext _context;
    private readonly IFaceService _faceService;
    private readonly IDistributedCache _cache;

    public AttendanceService(
        AppDbContext context,
        IFaceService faceService,
        IDistributedCache cache)
    {
        _context = context;
        _faceService = faceService;
        _cache = cache;
    }

    // ── Layer 1 & 2: Token Validation & Page Load Rate Limiting ──────────────────
    public async Task<ApiResponse<AttendanceTokenValidateResponse>> ValidateTokenAsync(string token, string ipAddress)
    {
        // Layer 1: Token Validation
        var tokenEntity = await _context.OnlineAttendanceTokens
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Session)
                .ThenInclude(s => s.Course)
                    .ThenInclude(c => c.Professor)
            .FirstOrDefaultAsync(t => t.Token == token && t.IsActive && t.ExpiresAt > DateTime.UtcNow);

        if (tokenEntity is null || !tokenEntity.Session.IsActive)
        {
            return new ApiResponse<AttendanceTokenValidateResponse>
            {
                Success = false,
                Message = "This attendance link is invalid or has expired."
            };
        }

        // Layer 2: Rate Limit (3 page loads per IP per token)
        var cacheKey = $"ratelimit:load:{token}:{ipAddress}";
        var pageLoads = await IncrementCacheKeyAsync(cacheKey, TimeSpan.FromMinutes(15));
        if (pageLoads > 3)
        {
            return new ApiResponse<AttendanceTokenValidateResponse>
            {
                Success = false,
                Message = "Too many page load attempts. Rate limit exceeded (Max 3/15m)."
            };
        }

        return new ApiResponse<AttendanceTokenValidateResponse>
        {
            Success = true,
            Message = "Token validated successfully.",
            Data = new AttendanceTokenValidateResponse
            {
                SessionId = tokenEntity.SessionId,
                CourseName = tokenEntity.Session.Course.Name,
                CourseCode = tokenEntity.Session.Course.CourseCode,
                ProfessorName = tokenEntity.Session.Course.Professor.FullName,
                ExpiresAt = tokenEntity.ExpiresAt
            }
        };
    }

    // ── Full Attendance Marking Pipeline (Layers 1-8) ───────────────────────────
    public async Task<ApiResponse<AttendanceMarkResponse>> MarkAttendanceAsync(AttendanceMarkRequest request, string ipAddress)
    {
        // ── Layer 1: Token Validation ───────────────────────────────────────────
        var tokenEntity = await _context.OnlineAttendanceTokens
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Session)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(t => t.Token == request.Token && t.IsActive && t.ExpiresAt > DateTime.UtcNow);

        if (tokenEntity is null || !tokenEntity.Session.IsActive)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Invalid or expired attendance session."
            };
        }

        // ── Layer 2: Rate Limiting (5 detection attempts per deviceId per token) ──
        var cacheKey = $"ratelimit:mark:{request.Token}:{request.DeviceId}";
        var markAttempts = await IncrementCacheKeyAsync(cacheKey, TimeSpan.FromMinutes(5));
        if (markAttempts > 5)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Too many marking attempts from this device. Rate limit exceeded (Max 5/5m)."
            };
        }

        // ── Layer 3 & 4: Face Detection & Face Identification ────────────────────
        var (rekognitionFaceId, similarity) = await _faceService.SearchFaceAsync(request.Image);
        
        // Layer 3: Face Detection failure
        if (rekognitionFaceId is null && similarity == 0)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "No face could be detected in the captured image. Please try again."
            };
        }

        // Layer 4: Face Identification failure (similarity must be >= 80%)
        if (rekognitionFaceId is null || similarity < 80.0)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Face verification failed. We could not match your face with any registered student template."
            };
        }

        // ── Layer 5: Student Lookup ──────────────────────────────────────────────
        var faceVector = await _context.FaceVectors
            .AsNoTracking()
            .Include(fv => fv.Student)
            .FirstOrDefaultAsync(fv => fv.RekognitionFaceId == rekognitionFaceId);

        if (faceVector is null)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Matched face template, but no matching student profile was found in the database."
            };
        }

        var student = faceVector.Student;

        if (student.EnrollmentStatus != "Trained")
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = $"Your face enrollment status is '{student.EnrollmentStatus}'. You must be fully 'Trained' to mark attendance."
            };
        }

        // ── Layer 6: Enrollment Check ────────────────────────────────────────────
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == student.Id && e.CourseId == tokenEntity.Session.CourseId);

        if (!isEnrolled)
        {
            // Log Abuse
            var abuse = new AbuseLog
            {
                SessionId = tokenEntity.SessionId,
                StudentId = student.Id,
                AbuseType = AppConstants.AbuseTypes.NotEnrolledInCourse,
                Details = $"Student '{student.FullName}' (Roll: {student.RollNumber}) attempted to mark attendance for course '{tokenEntity.Session.Course.Name}' but is not enrolled.",
                DeviceId = request.DeviceId,
                IpAddress = ipAddress
            };
            _context.AbuseLogs.Add(abuse);
            await _context.SaveChangesAsync();

            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = $"Access Denied: You are not officially enrolled in the course '{tokenEntity.Session.Course.Name}'."
            };
        }

        // ── Layer 7: Duplicate Check ─────────────────────────────────────────────
        var alreadyMarked = await _context.AttendanceRecords
            .AnyAsync(ar => ar.SessionId == tokenEntity.SessionId && ar.StudentId == student.Id);

        if (alreadyMarked)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Your attendance has already been successfully marked for this session."
            };
        }

        // ── Layer 8: Insert AttendanceRecord ─────────────────────────────────────
        var record = new AttendanceRecord
        {
            SessionId = tokenEntity.SessionId,
            StudentId = student.Id,
            Confidence = (float)similarity,
            DeviceId = request.DeviceId,
            MarkedAt = DateTime.UtcNow
        };

        _context.AttendanceRecords.Add(record);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new ApiResponse<AttendanceMarkResponse>
            {
                Success = false,
                Message = "Your attendance has already been successfully marked for this session."
            };
        }

        return new ApiResponse<AttendanceMarkResponse>
        {
            Success = true,
            Message = "Attendance marked successfully!",
            Data = new AttendanceMarkResponse
            {
                Success = true,
                StudentName = student.FullName,
                RollNumber = student.RollNumber,
                CourseName = tokenEntity.Session.Course.Name,
                CourseCode = tokenEntity.Session.Course.CourseCode,
                MarkedAt = record.MarkedAt
            }
        };
    }

    // ── Distributed Cache Helper ────────────────────────────────────────────────
    private async Task<int> IncrementCacheKeyAsync(string key, TimeSpan expiry)
    {
        var keyLock = CacheKeyLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await keyLock.WaitAsync();
        try
        {
            var valStr = await _cache.GetStringAsync(key);
            int val = 0;
            if (valStr != null && int.TryParse(valStr, out var parsed))
            {
                val = parsed;
            }
            val++;
            await _cache.SetStringAsync(key, val.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
            return val;
        }
        finally
        {
            keyLock.Release();
        }
    }
}
