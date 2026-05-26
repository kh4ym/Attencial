namespace Attencial.Shared.Dtos;

public class CreateSessionRequest
{
    public int CourseId { get; set; }
    public int ExpiryMinutes { get; set; }  // 5, 10, 15, or 30
}
