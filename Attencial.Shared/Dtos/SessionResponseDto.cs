namespace Attencial.Shared.Dtos;

public class SessionResponseDto
{
    public int SessionId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AttendanceUrl { get; set; } = string.Empty;
}
