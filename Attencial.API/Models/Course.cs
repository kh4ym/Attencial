namespace Attencial.API.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public int ProfessorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Professor Professor { get; set; } = null!;
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<EnrollmentRequest> EnrollmentRequests { get; set; } = new();
    public List<AttendanceSession> AttendanceSessions { get; set; } = new();
}
