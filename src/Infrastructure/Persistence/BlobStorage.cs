using Application.Contracts.Persistence;
using Application.DTOs.Media;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentResults;
using Microsoft.AspNetCore.Http;
// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Infrastructure.Persistence;

internal sealed class BlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorage(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Result<Guid>> UploadFileAsync(Guid mediaId, IFormFile fileStream, string contentType, string containerName, CancellationToken ct = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(mediaId.ToString());

        try
        {
            await using var stream = fileStream.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = contentType
            }, cancellationToken: ct);
            
        }
        catch (Azure.RequestFailedException e)
        {
            Console.WriteLine(e.Message);
            return Result.Fail(new Error("FailedToUploadBlob").WithMetadata("Status", e.Status).CausedBy(e));
        }

        return Result.Ok(mediaId);
    }

    public async Task<Result<Stream>> GetFileAsync(Guid mediaId, string containerName, CancellationToken ct = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        var blobClient = containerClient.GetBlobClient(mediaId.ToString());

        try
        {
            var response = await blobClient.DownloadContentAsync(cancellationToken: ct);
            
            if (response.Value.Content is null)
                return Result.Fail(new Error("BlobFileNotFound").WithMetadata("Status", StatusCodes.Status404NotFound));

            // returning the file stream and content type from the blob
            return Result.Ok(response.Value.Content.ToStream());
        }
        catch (Azure.RequestFailedException e)
        {
            Console.WriteLine(e.Message);
            return Result.Fail(new Error("FailedToDownloadBlob").WithMetadata("Status", e.Status).CausedBy(e));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return Result.Fail(new Error("UnexpectedBlobDownloadError").CausedBy(e));
        }
    }

    public async Task DeleteFileAsync(Guid mediaId, string containerName, CancellationToken ct = default)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        
        var blobClient = containerClient.GetBlobClient(mediaId.ToString());

        try
        {
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Azure.RequestFailedException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}