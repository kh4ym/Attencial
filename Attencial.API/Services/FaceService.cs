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
    private bool _collectionEnsured;

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
                FailureRatio = 1.0,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
    }

    private async Task EnsureCollectionExistsAsync()
    {
        if (_collectionEnsured) return;

        try
        {
            await _client.CreateCollectionAsync(
                new CreateCollectionRequest { CollectionId = _collectionId });
        }
        catch (ResourceAlreadyExistsException)
        {
            // Collection already exists
        }

        _collectionEnsured = true;
    }

    public async Task<string?> DetectFaceAsync(string base64Image)
    {
        await EnsureCollectionExistsAsync();

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

            if (response.FaceDetails.Count == 0)
                return null;

            return "detected";
        });
    }

    public async Task<string> IndexFaceAsync(string base64Image, string externalId)
    {
        await EnsureCollectionExistsAsync();

        return await _resiliencePipeline.ExecuteAsync(async token =>
        {
            var imageBytes = Convert.FromBase64String(base64Image);

            var response = await _client.IndexFacesAsync(new IndexFacesRequest
            {
                CollectionId    = _collectionId,
                ExternalImageId = externalId,
                Image = new Image
                {
                    Bytes = new MemoryStream(imageBytes)
                },
                MaxFaces        = 1,
                QualityFilter   = QualityFilter.AUTO
            }, token);

            if (response.FaceRecords.Count == 0)
                throw new InvalidOperationException("No face could be indexed in this image.");

            return response.FaceRecords[0].Face.FaceId;
        });
    }

    public async Task DeleteFaceAsync(string rekognitionFaceId)
    {
        await EnsureCollectionExistsAsync();

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
        await EnsureCollectionExistsAsync();

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
                        FaceMatchThreshold = 70F
                    }, token);
            }
            catch (InvalidParameterException)
            {
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
