namespace Attencial.API.Models;

public class FacultyAbuseLog
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public string AbuseType { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Professor Professor { get; set; } = null!;
}
