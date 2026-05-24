namespace Attencial.Shared.Dtos;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "Student";
    public string FullName { get; init; } = string.Empty;
    public string RollNumber { get; init; } = string.Empty;
}
