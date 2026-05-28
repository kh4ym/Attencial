namespace Attencial.Shared.Dtos;

public class FacultyFaceScanRequest
{
    public string? Image { get; set; }   // single image (direct API)
    public List<string>? Images { get; set; }  // multiple images (FaceCaptureComponent)
}
