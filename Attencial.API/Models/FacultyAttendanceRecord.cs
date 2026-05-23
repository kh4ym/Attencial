namespace Attencial.API.Models;

public class FacultyAttendanceRecord
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? HoursWorked { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Professor Professor { get; set; } = null!;
}
