using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Attencial.API.Services;

public class FaceService : IFaceService
{
    private readonly AmazonRekognitionClient _client;
    private readonly string _collectionId;
    private readonly ResiliencePipeline _resiliencePipeline;

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

        // Build resilience pipeline (3x exponential retry + circuit breaker on 5 failures)
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                    ex is not InvalidParameterException && ex is not ResourceAlreadyExistsException),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1)
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                    ex is not InvalidParameterException && ex is not ResourceAlreadyExistsException),
                FailureRatio = 1.0,               // Trip if 100% of calls fail
                MinimumThroughput = 5,            // Needs at least 5 failures to trip
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
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
        return await _resiliencePipeline.ExecuteAsync(async token =>
        {
            var imageBytes = Convert.FromBase64String(base64Image);

            var response = await _client.DetectFacesAsync(new DetectFacesRequest
            {
                Image = new Image
                {
                    Bytes = new MemoryStream(imageBytes)
                }
            }, token);

            // No face found?
            if (response.FaceDetails.Count == 0)
                return null;

            // Return a confirmation token — "detected"
            return "detected";
        });
    }

    public async Task<string> IndexFaceAsync(string base64Image, string externalId)
    {
        return await _resiliencePipeline.ExecuteAsync(async token =>
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
            }, token);

            if (response.FaceRecords.Count == 0)
                throw new InvalidOperationException("No face could be indexed in this image.");

            // Return the Rekognition-assigned FaceId (a GUID string)
            return response.FaceRecords[0].Face.FaceId;
        });
    }

    public async Task DeleteFaceAsync(string rekognitionFaceId)
    {
        await _resiliencePipeline.ExecuteAsync(async token =>
        {
            await _client.DeleteFacesAsync(new DeleteFacesRequest
            {
                CollectionId = _collectionId,
                FaceIds      = new List<string> { rekognitionFaceId }
            }, token);
        });
    }

    public async Task<(string? faceId, double similarity)> SearchFaceAsync(string base64Image)
    {
        return await _resiliencePipeline.ExecuteAsync(async token =>
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
                    }, token);
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
        });
    }
}
