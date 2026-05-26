using System;

namespace Attencial.Shared.Dtos;

public class LeaveRequestCreateRequest
{
    public string LeaveType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? AttachmentBase64 { get; set; } // PDF encoded string
    public string? AttachmentFileName { get; set; }
}
