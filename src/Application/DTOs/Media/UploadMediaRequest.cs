using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Media;

public sealed class UploadMediaRequest
{
    public required Guid EntityId { get; init; }
    public required IFormFile File { get; init; }
}