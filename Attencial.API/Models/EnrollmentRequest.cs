namespace Attencial.API.Models;

public class EnrollmentRequest
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }

    /// <summary>"Pending" | "Approved" | "Rejected"</summary>
    public string Status { get; set; } = "Pending";

    /// <summary>Optional note from the professor (e.g. rejection reason).</summary>
    public string? Note { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
    public Course Course  { get; set; } = null!;
}
