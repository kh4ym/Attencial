using System.Threading.Tasks;
using Attencial.Shared.Dtos;

namespace Attencial.API.Services;

/// <summary>
/// Core attendance marking pipeline with multi-layer validation:
/// token validation, rate limiting, face detection/identification,
/// student lookup, enrollment verification, and duplicate prevention.
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// Validates an attendance token and applies page-load rate limiting (3 loads per IP per 15m).
    /// </summary>
    Task<ApiResponse<AttendanceTokenValidateResponse>> ValidateTokenAsync(string token, string ipAddress);

    /// <summary>
    /// Executes the full 8-layer attendance marking pipeline:
    /// Token validation → Rate limiting → Face detection → Face identification →
    /// Student lookup → Enrollment check → Duplicate check → Record insertion.
    /// </summary>
    Task<ApiResponse<AttendanceMarkResponse>> MarkAttendanceAsync(AttendanceMarkRequest request, string ipAddress);
}
