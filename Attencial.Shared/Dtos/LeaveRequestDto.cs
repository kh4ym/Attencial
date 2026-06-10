using System;

namespace Attencial.Shared.Dtos;

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Type { get; set; } = "Sick";
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}