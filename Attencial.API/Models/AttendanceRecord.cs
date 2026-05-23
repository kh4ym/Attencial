namespace Attencial.API.Models;

public class AttendanceRecord
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int StudentId { get; set; }
    public float Confidence { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;

    public AttendanceSession Session { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
