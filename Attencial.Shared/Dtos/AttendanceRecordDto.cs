using System;

namespace Attencial.Shared.Dtos;

public class AttendanceRecordDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Status { get; set; } = "Present";
    public string MarkedBy { get; set; } = string.Empty;
}