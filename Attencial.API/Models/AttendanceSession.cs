namespace Attencial.API.Models;

public class AttendanceSession
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int ProfessorId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Course Course { get; set; } = null!;
    public OnlineAttendanceToken? Token { get; set; }
    public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
}
