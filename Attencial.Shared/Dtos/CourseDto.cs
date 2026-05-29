namespace Attencial.Shared.Dtos;

/// <summary>
/// Course listing DTO returned to students.
/// Includes the student's own enrollment/request status for that course.
/// </summary>
public class CourseDto
{
    public int    Id                      { get; set; }
    public string Name                    { get; set; } = string.Empty;
    public string CourseCode              { get; set; } = string.Empty;
    public string ProfessorName           { get; set; } = string.Empty;
    public string Department              { get; set; } = string.Empty;

    /// <summary>
    /// "None" | "Pending" | "Approved" | "Rejected"
    /// Populated relative to the requesting student.
    /// </summary>
    public string EnrollmentRequestStatus { get; set; } = "None";

    /// <summary>Professor's note (visible when Status = "Rejected").</summary>
    public string? Note { get; set; }
}
