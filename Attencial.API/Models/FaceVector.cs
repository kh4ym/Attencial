namespace Attencial.API.Models;

public class FaceVector
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string AzurePersonId { get; set; } = string.Empty;
    public string AzureFaceId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; } = null!;
}
