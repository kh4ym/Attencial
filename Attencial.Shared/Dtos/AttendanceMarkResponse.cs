using System;

namespace Attencial.Shared.Dtos;

public class AttendanceMarkResponse
{
    public bool Success { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public DateTime MarkedAt { get; set; }
}
