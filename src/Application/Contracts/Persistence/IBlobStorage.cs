using Application.DTOs.Media;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Application.Contracts.Persistence;

// Interface for basic crud operations on videos/images in Azure blob storage
public interface IBlobStorage
{
    Task<Result<Guid>>  UploadFileAsync(Guid mediaId, IFormFile fileStream, string contentType, string containerName, CancellationToken ct = default);

    Task<Result<Stream>> GetFileAsync(Guid mediaId, string containerName, CancellationToken ct = default);
    
    Task DeleteFileAsync(Guid mediaId, string containerName, CancellationToken ct = default);
}