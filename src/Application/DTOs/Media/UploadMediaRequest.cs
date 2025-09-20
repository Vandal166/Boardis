using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Media;

public sealed class UploadMediaRequest
{
    public required IFormFile File { get; init; }
}