namespace Attencial.API.Services;

public interface IFaceService
{
    Task<string?> DetectFaceAsync(string base64Image);
    Task<string> IndexFaceAsync(string base64Image, string externalId);
    Task DeleteFaceAsync(string rekognitionFaceId);
    Task<(string? faceId, double similarity)> SearchFaceAsync(string base64Image);
}
