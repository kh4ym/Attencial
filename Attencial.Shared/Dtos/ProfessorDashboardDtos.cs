using System;

namespace Attencial.Shared.Dtos;

public class ProfessorSessionDto
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public double AttendanceRate { get; set; }
}

public class AbuseLogResponseDto
{
    public int Id { get; set; }
    public int? SessionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string AbuseType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime LoggedAt { get; set; }
}
