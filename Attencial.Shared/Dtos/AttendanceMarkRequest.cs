using System;
using System.Collections.Generic;

namespace Attencial.Shared.Dtos;

public class AttendanceMarkRequest
{
    public int CourseId { get; set; }
    public DateTime Date { get; set; }
    public List<StudentAttendanceMarkDto> Markings { get; set; } = new();
}

public class StudentAttendanceMarkDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Status { get; set; } = "Present";
}