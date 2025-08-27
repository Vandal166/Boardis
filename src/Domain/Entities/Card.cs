using FluentResults;

namespace Domain.Entities;

public sealed class Card
{
    public Guid Id { get; private set; }
    public Guid ListId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public bool IsCompleted { get; private set; } = false;
    public int Position { get; private set; } // position/order of the card in the list

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Card() { }

    public static Result<Card> Create(Guid listId, string title, string? description = null)
    {
        if (listId == Guid.Empty)
            return Result.Fail<Card>("List ID cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<Card>("Title cannot be empty.");

        if (title.Length > 100)
            return Result.Fail<Card>("Title cannot exceed 100 characters.");

        if (description is { Length: > 500 })
            return Result.Fail<Card>("Description cannot exceed 500 characters.");

        var card = new Card
        {
            Id = Guid.NewGuid(),
            ListId = listId,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Ok(card);
    }
    
    public Result ToggleCompletion()
    {
        IsCompleted = !IsCompleted;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
}