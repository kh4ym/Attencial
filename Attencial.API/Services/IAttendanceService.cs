using System.Threading.Tasks;
using Attencial.Shared.Dtos;

namespace Attencial.API.Services;

public interface IAttendanceService
{
    Task<ApiResponse<AttendanceTokenValidateResponse>> ValidateTokenAsync(string token, string ipAddress);
    Task<ApiResponse<AttendanceMarkResponse>> MarkAttendanceAsync(AttendanceMarkRequest request, string ipAddress);
}
