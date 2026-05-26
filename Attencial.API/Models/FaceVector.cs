using System;

namespace Attencial.API.Models;

public class FaceVector
{
    public int Id { get; set; }
    public int? StudentId { get; set; }
    public int? ProfessorId { get; set; }
    public string RekognitionExternalId { get; set; } = string.Empty;
    public string RekognitionFaceId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; set; }
    public Professor? Professor { get; set; }
}
