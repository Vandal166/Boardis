using System.Text.Json.Serialization;
using Domain.Common;
using FluentResults;

namespace Domain.Entities;

public sealed class ListCard
{
    public Guid Id { get; private set; }
    public Guid BoardListId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime? CompletedAt { get; private set; } // null if not completed, timestamp if completed
    public double Position { get; private set; } // position/order of the card in the list

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ListCard() { }

    public static Result<ListCard> Create(Guid ListId, string Title, double Position, string? Description = null)
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
    
    public Result Patch(PatchValue<string?> title, PatchValue<double?> position, PatchValue<string?> description, PatchValue<DateTime?> completedAt)
    {
        var errors = new List<IError>();

        if (title.IsSet)
        {
            if(title.Value is null) //TODO is this check necessary? Since UpdatePosition checks anyway if null/empty
            {
                errors.Add(new Error("Title cannot be null").WithMetadata("PropertyName", nameof(Title)));
            }
            else
            {
                var titleResult = UpdateTitle(title.Value);
                if (titleResult.IsFailed)
                    errors.AddRange(titleResult.Errors);
            }
        }

        if (position.IsSet)
        {
            if(position.Value is null)
            {
                errors.Add(new Error("Position cannot be null").WithMetadata("PropertyName", nameof(Position)));
            }
            else
            {
                var positionResult = UpdatePosition(position.Value.Value);
                if (positionResult.IsFailed)
                    errors.AddRange(positionResult.Errors);
            }
        }

        if (description.IsSet)
        {
            var descriptionResult = UpdateDescription(description.Value);
            if (descriptionResult.IsFailed)
                errors.AddRange(descriptionResult.Errors);
        }

        if (completedAt.IsSet)
        {
            var completedAtResult = UpdateCompletedAt(completedAt.Value);
            if (completedAtResult.IsFailed)
                errors.AddRange(completedAtResult.Errors);
        }

        if (errors.Count != 0)
            return Result.Fail(errors);

        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    
    private Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));

        if (title.Length > 100)
            return Result.Fail(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));

        this.Title = title;
        return Result.Ok();
    }
    
    private Result UpdateDescription(string? description)
    {
        if (description is { Length: > 500 })
            return Result.Fail(new Error("Description cannot exceed 500 characters.").WithMetadata("PropertyName", nameof(Description)));

        this.Description = description;
        return Result.Ok();
    }
    
    private Result UpdatePosition(double position)
    {
        if (position < 0)
            return Result.Fail(new Error("Position cannot be negative.").WithMetadata("PropertyName", nameof(Position)));
        
        this.Position = position;
        return Result.Ok();
    }
    
    private Result UpdateCompletedAt(DateTime? completedAt)
    {
        if( completedAt is not null && completedAt > DateTime.UtcNow)
            return Result.Fail(new Error("CompletedAt cannot be in the future.").WithMetadata("PropertyName", nameof(CompletedAt)));

        this.CompletedAt = completedAt;
        return Result.Ok();
    }
    
    [JsonConstructor]
    private ListCard(Guid id, Guid boardListId, string title, string? description, DateTime? completedAt, double position, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        BoardListId = boardListId;
        Title = title;
        Description = description;
        CompletedAt = completedAt;
        Position = position;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}