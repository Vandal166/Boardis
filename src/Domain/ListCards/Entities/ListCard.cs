using Domain.Common;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.ListCards.Entities;

public sealed class ListCard : Entity
{
    public Guid Id { get; private set; }
    public Guid BoardListId { get; private set; }
    public Title Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime? CompletedAt { get; private set; } // null if not completed, timestamp if completed
    public double Position { get; private set; } // position/order of the card in the list

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private ListCard() { }

    internal static Result<ListCard> Create(Guid ListId, string Title, string? Description, double Position)
    {
        if (ListId == Guid.Empty)
            return Result.Fail<ListCard>(new Error("ListIdRequired").WithMetadata("PropertyName", nameof(ListId)));
        
        var titleResult = ValueObjects.Title.TryFrom(Title);
        if (!titleResult.IsSuccess)
            return Result.Fail<ListCard>("TitleInvalid");
        
        var card = new ListCard
        {
            Id = Guid.NewGuid(),
            BoardListId = ListId,
            Title = titleResult.ValueObject,
            Description = Description,
            Position = Position,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Ok(card);
    }
    
    internal Result Patch(PatchValue<string?> title, PatchValue<double?> position, PatchValue<string?> description, PatchValue<DateTime?> completedAt)
    {
        var errors = new List<IError>();

        if (title.IsSet)
        {
            if(title.Value is null)
            {
                errors.Add(new Error("TitleCannotBeNull").WithMetadata("PropertyName", nameof(Title)));
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
                errors.Add(new Error("PositionCannotBeNull").WithMetadata("PropertyName", nameof(Position)));
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
        var titleResult = Title.TryFrom(title);
        if (!titleResult.IsSuccess)
            return Result.Fail("TitleInvalid");

        this.Title = titleResult.ValueObject;
        return Result.Ok();
    }
    
    private Result UpdateDescription(string? description)
    {
        if (description is { Length: > 500 })
            return Result.Fail(new Error("DescriptionTooLong").WithMetadata("PropertyName", nameof(Description)));
        
        this.Description = description;
        return Result.Ok();
    }

    private Result UpdatePosition(double position)
    {
        if (position < 0)
            return Result.Fail(new Error("PositionMustBeNonNegative").WithMetadata("PropertyName", nameof(Position)));
        
        this.Position = position;
        return Result.Ok();
    }
    
    private Result UpdateCompletedAt(DateTime? completedAt)
    {
        if( completedAt is not null && completedAt > DateTime.UtcNow)
            return Result.Fail(new Error("CompletedAtCannotBeInFuture").WithMetadata("PropertyName", nameof(CompletedAt)));

        this.CompletedAt = completedAt;
        return Result.Ok();
    }
}