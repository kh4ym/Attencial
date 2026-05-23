namespace Attencial.API.Models;

public class OnlineAttendanceToken
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AttendanceSession Session { get; set; } = null!;
}
