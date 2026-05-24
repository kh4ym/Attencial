using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.IO;
using System.Text.Json;

namespace Attencial.API.Services;

public class FaceService : IFaceService
{
    private readonly AmazonRekognitionClient _client;
    private readonly string _collectionId;

    public FaceService(IConfiguration config)
    {
        var accessKey = config["AwsRekognition:AccessKey"]!;
        var secretKey = config["AwsRekognition:SecretKey"]!;
        var region    = config["AwsRekognition:Region"]!;
        _collectionId = config["AwsRekognition:CollectionId"]!;

        _client = new AmazonRekognitionClient(
            accessKey,
            secretKey,
            RegionEndpoint.GetBySystemName(region));

        // Ensure the collection exists (idempotent — safe to call every startup)
        EnsureCollectionExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureCollectionExistsAsync()
    {
        try
        {
            await _client.CreateCollectionAsync(
                new CreateCollectionRequest { CollectionId = _collectionId });
        }
        catch (ResourceAlreadyExistsException)
        {
            // Collection already exists — that's fine, do nothing
        }
    }

    public async Task<string?> DetectFaceAsync(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);

        var response = await _client.DetectFacesAsync(new DetectFacesRequest
        {
            Image = new Image
            {
                Bytes = new MemoryStream(imageBytes)
            }
        });

        // No face found?
        if (response.FaceDetails.Count == 0)
            return null;

        // Return a confirmation token — "detected"
        return "detected";
    }

    public async Task<string> IndexFaceAsync(string base64Image, string externalId)
    {
        var imageBytes = Convert.FromBase64String(base64Image);

        var response = await _client.IndexFacesAsync(new IndexFacesRequest
        {
            CollectionId    = _collectionId,
            ExternalImageId = externalId,   // e.g. student's roll number
            Image = new Image
            {
                Bytes = new MemoryStream(imageBytes)
            },
            MaxFaces        = 1,
            QualityFilter   = QualityFilter.AUTO
        });

        if (response.FaceRecords.Count == 0)
            throw new InvalidOperationException("No face could be indexed in this image.");

        // Return the Rekognition-assigned FaceId (a GUID string)
        return response.FaceRecords[0].Face.FaceId;
    }

    public async Task DeleteFaceAsync(string rekognitionFaceId)
    {
        await _client.DeleteFacesAsync(new DeleteFacesRequest
        {
            CollectionId = _collectionId,
            FaceIds      = new List<string> { rekognitionFaceId }
        });
    }

    public async Task<(string? faceId, double similarity)> SearchFaceAsync(string base64Image)
    {
        var imageBytes = Convert.FromBase64String(base64Image);

        SearchFacesByImageResponse response;
        try
        {
            response = await _client.SearchFacesByImageAsync(
                new SearchFacesByImageRequest
                {
                    CollectionId       = _collectionId,
                    Image = new Image
                    {
                        Bytes = new MemoryStream(imageBytes)
                    },
                    MaxFaces           = 1,
                    FaceMatchThreshold = 70F  // pre-filter at 70% on AWS side
                });
        }
        catch (InvalidParameterException)
        {
            // No face detected in the query image
            return (null, 0);
        }

        if (response.FaceMatches.Count == 0)
            return (null, 0);

        var match      = response.FaceMatches[0];
        var faceId     = match.Face.FaceId;
        var similarity = (double)(match.Similarity ?? 0f);

        return (faceId, similarity);
    }
}
