namespace Attencial.API.Services;

/// <summary>
/// Abstraction over AWS Rekognition for face detection, indexing, and search operations.
/// </summary>
public interface IFaceService
{
    /// <summary>
    /// Detects whether a face is present in the given image. Returns null if no face found.
    /// </summary>
    Task<string?> DetectFaceAsync(string base64Image);

    /// <summary>
    /// Indexes a face into the Rekognition collection and returns the assigned FaceId.
    /// </summary>
    Task<string> IndexFaceAsync(string base64Image, string externalId);

    /// <summary>
    /// Deletes a face from the Rekognition collection by its FaceId.
    /// </summary>
    Task DeleteFaceAsync(string rekognitionFaceId);

    /// <summary>
    /// Searches for a matching face in the Rekognition collection.
    /// Returns the FaceId and similarity score (0-100). Returns (null, 0) if no match found.
    /// </summary>
    Task<(string? faceId, double similarity)> SearchFaceAsync(string base64Image);
}
