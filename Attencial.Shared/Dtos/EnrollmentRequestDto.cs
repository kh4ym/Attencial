namespace Attencial.Shared.Dtos;

/// <summary>
/// Enrollment request DTO returned to professors in the review queue.
/// </summary>
public class EnrollmentRequestDto
{
    public int      Id           { get; set; }
    public int      StudentId    { get; set; }
    public string   StudentName  { get; set; } = string.Empty;
    public string   RollNumber   { get; set; } = string.Empty;
    public int      CourseId     { get; set; }
    public string   CourseName   { get; set; } = string.Empty;
    public string   CourseCode   { get; set; } = string.Empty;
    public string   Status       { get; set; } = string.Empty;
    public string?  Note         { get; set; }
    public DateTime RequestedAt  { get; set; }
    public DateTime? ReviewedAt  { get; set; }
}
