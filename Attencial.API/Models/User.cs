namespace Attencial.API.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // A User MAY be a Student or Professor (nullable = optional)
    public Student? Student { get; set; }
    public Professor? Professor { get; set; }
}