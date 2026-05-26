using System;

namespace Attencial.Shared.Dtos;

public class LeaveRequestResponseDto
{
    public int Id { get; set; }
    public string ProfessorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
}
