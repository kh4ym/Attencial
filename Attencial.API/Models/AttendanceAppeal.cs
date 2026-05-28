namespace Attencial.API.Models;

public class AttendanceAppeal
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SessionId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
}
