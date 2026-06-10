using System;

namespace Attencial.Shared.Dtos;

public class CourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ProfessorName { get; set; } = string.Empty;
    public string ProfessorEmail { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public double AttendanceRate { get; set; }
    public int TotalLectures { get; set; }
    public int AttendedLectures { get; set; }
}