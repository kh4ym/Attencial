using System;
using System.Collections.Generic;

namespace Attencial.API.Models;

public class Professor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public List<Course> Courses { get; set; } = new();
    public List<FacultyAttendanceRecord> FacultyAttendanceRecords { get; set; } = new();
    public List<FaceVector> FaceVectors { get; set; } = new();
}
