using Domain.Common;
using FluentResults;

namespace Domain.Images.Entities;

public sealed class Media : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public Guid BoundToEntityId { get; private set; } // e.g. BoardId, UserId, etc.
    
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }

    private Media() { }
    
    public static Result<Media> Create(Guid mediaId, Guid boundToEntityId, Guid uploadedByUserId)
    {
        if (mediaId == Guid.Empty)
            return Result.Fail<Media>("Media ID cannot be empty.");
        
        if (boundToEntityId == Guid.Empty)
            return Result.Fail<Media>("Bound to entity ID cannot be empty.");
        
        var image = new Media
        {
            Id = mediaId,
            BoundToEntityId = boundToEntityId,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };
        
        return Result.Ok(image);
    }
}