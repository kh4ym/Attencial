using System;

namespace Attencial.Shared.Dtos;

public class AttendanceTokenValidateResponse
{
    public int SessionId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string ProfessorName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
