using FluentResults;

namespace Domain.Entities;

public sealed class ListCard
{
    public Guid Id { get; private set; }
    public Guid BoardListId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime? CompletedAt { get; private set; } // null if not completed, timestamp if completed
    public int Position { get; private set; } // position/order of the card in the list

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ListCard() { }

    public static Result<ListCard> Create(Guid ListId, string Title, int Position, string? Description = null)
    {
        var errors = new List<Error>();
        if (ListId == Guid.Empty)
            errors.Add(new Error("List ID cannot be empty.").WithMetadata("PropertyName", nameof(ListId)));

        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));

        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));

        if (Description is { Length: > 500 })
            errors.Add(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));

        if (errors.Count != 0)
            return Result.Fail<ListCard>(errors);
        
        var card = new ListCard
        {
            Id = Guid.NewGuid(),
            BoardListId = ListId,
            Title = Title,
            Description = Description,
            Position = Position,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Ok(card);
    }
    
    public Result Update(string Title, int Position, string? Description = null, DateTime? CompletedAt = null)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));

        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));

        if (Description is { Length: > 500 })
            errors.Add(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));

        if( CompletedAt is not null && CompletedAt > DateTime.UtcNow)
            errors.Add(new Error("CompletedAt cannot be in the future.").WithMetadata("PropertyName", nameof(CompletedAt)));
        
        if (errors.Count != 0)
            return Result.Fail(errors);

        this.Title = Title;
        this.Position = Position;
        this.Description = Description;
        this.CompletedAt = CompletedAt;
        UpdatedAt = DateTime.UtcNow;

        return Result.Ok();
    }
    
    public Result MarkAsCompleted(DateTime? completedAt)
    {
        if (completedAt is not null && completedAt > DateTime.UtcNow)
            return Result.Fail(new Error("CompletedAt cannot be in the future.").WithMetadata("PropertyName", nameof(CompletedAt)));

        CompletedAt = completedAt;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
}