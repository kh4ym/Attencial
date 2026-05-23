namespace Attencial.API.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = "Pending";
    public string? AdminNote { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Professor Professor { get; set; } = null!;
}
