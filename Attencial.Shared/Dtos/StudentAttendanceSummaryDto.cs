using System;
using System.Collections.Generic;

namespace Attencial.Shared.Dtos;

public class StudentAttendanceSummaryDto
{
    public double OverallPercentage { get; set; }
    public int TotalCourses { get; set; }
    public int PresentSessions { get; set; }
    public int TotalSessions { get; set; }
    public List<StudentCourseAttendanceDto> CourseAttendance { get; set; } = new();
}

public class StudentCourseAttendanceDto
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string ProfessorName { get; set; } = string.Empty;
    public int TotalSessions { get; set; }
    public int AttendedSessions { get; set; }
    public double Percentage { get; set; }
    public string Status { get; set; } = string.Empty; // "Green", "Yellow", "Red"
    public List<MissedSessionDto> MissedSessions { get; set; } = new();
}

public class MissedSessionDto
{
    public int SessionId { get; set; }
    public DateTime Date { get; set; }
}
