namespace Application.DTOs.Media;

public sealed record MediaResponse
{
    public required Guid Id { get; init; }
    public required Guid BoundToEntityId { get; init; }
    
    public required byte[] Data { get; init; }
    public required DateTime UploadedAt { get; init; }
    public required Guid UploadedByUserId { get; init; }
}