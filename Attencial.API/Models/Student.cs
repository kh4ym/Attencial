namespace Attencial.API.Models;

public class Student
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string? AzurePersonId { get; set; }
    public string EnrollmentStatus { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<FaceVector> FaceVectors { get; set; } = new();
    public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
}