namespace Attencial.API.Models;

public class AbuseLog
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public int? StudentId { get; set; }
    public string AbuseType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AttendanceSession? Session { get; set; }
    public Student? Student { get; set; }
}
