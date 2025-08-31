using FluentResults;

namespace Domain.Entities;

public sealed class ListCard
{
    public Guid Id { get; private set; }
    public Guid BoardListId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
   // public bool IsCompleted { get; private set; } = false; //TODO CompletedAt instead
    public DateTime? CompletedAt { get; private set; } // null if not completed, timestamp if completed
    public int Position { get; private set; } // position/order of the card in the list

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ListCard() { }

    public static Result<ListCard> Create(Guid listId, string title, string? description = null)
    {
        if (listId == Guid.Empty)
            return Result.Fail<ListCard>("List ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<ListCard>("Title cannot be empty.");

        if (title.Length > 100)
            return Result.Fail<ListCard>("Title cannot exceed 100 characters.");

        if (description is { Length: > 500 })
            return Result.Fail<ListCard>("Description cannot exceed 500 characters.");

        var card = new ListCard
        {
            Id = Guid.NewGuid(),
            BoardListId = listId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Ok(card);
    }
    
    public Result ToggleCompletion()
    {
        //IsCompleted = !IsCompleted;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
}