using System.Drawing;
using Domain.Common;
using Domain.ListCards.Entities;
using Domain.ListCards.Events;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.BoardLists.Entities;

public sealed class BoardList : Entity
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Title Title { get; private set; }
    public double Position { get; private set; } // position/order of the list on the board
    public int ListColorArgb { get; private set; } = Color.LightBlue.ToArgb();
    public Color ListColor
    {
        get => Color.FromArgb(ListColorArgb);
        private set => ListColorArgb = value.ToArgb();
    }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public IReadOnlyCollection<ListCard> Cards => _cards.AsReadOnly();
    private readonly List<ListCard> _cards = new();
    
    private BoardList() { }
    
    internal static Result<BoardList> Create(Guid BoardId, string Title, double Position)
    {
        if (BoardId == Guid.Empty)
            return Result.Fail<BoardList>(new Error("BoardIdRequired").WithMetadata("PropertyName", nameof(BoardId)));
        
        var titleResult = ValueObjects.Title.TryFrom(Title);
        if (!titleResult.IsSuccess)
            return Result.Fail<BoardList>("TitleInvalid");
        
        return Result.Ok(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = BoardId,
            Title = titleResult.ValueObject,
            Position = Position,
            CreatedAt = DateTime.UtcNow
        });
    }
    internal Result Patch(PatchValue<string?> title, PatchValue<double?> position, PatchValue<int?> colorArgb)
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
        
        if (colorArgb.IsSet)
        {
            if(colorArgb.Value is null)
            {
                //set default color if null
                this.ListColor = Color.LightBlue;
            }
            else
            {
                var colorResult = UpdateColor(colorArgb.Value.Value);
                if (colorResult.IsFailed)
                    errors.AddRange(colorResult.Errors);
            }
        }
        
        if (errors.Count != 0)
            return Result.Fail(errors);
        
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result<ListCard> AddCard(Guid requestingUserId, string title, string? description, double position)
    {
        var cardResult = ListCard.Create(Id, title, description, position);
        if (cardResult.IsFailed)
            return Result.Fail<ListCard>(cardResult.Errors);
        
        _cards.Add(cardResult.Value);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ListCardCreatedEvent(BoardId, Id, cardResult.Value.Id, requestingUserId));
        return cardResult;
    }

    public Result RemoveCard(Guid cardId, Guid deletedById)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Fail(new Error("CardNotFound").WithMetadata("Status", 404));

        _cards.Remove(card);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ListCardDeletedEvent(BoardId, Id, card.Id, deletedById));
        return Result.Ok();
    }

    public Result PatchCard(Guid cardId, Guid updatedById, PatchValue<string?> title, PatchValue<double?> position, PatchValue<string?> description,
        PatchValue<DateTime?> completedAt)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Fail(new Error("CardNotFound").WithMetadata("Status", 404));
        
        var updateResult = card.Patch(title, position, description, completedAt);
        if (updateResult.IsFailed)
            return Result.Fail(updateResult.Errors);
        
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new ListCardUpdatedEvent(BoardId, Id, card.Id, updatedById));
        return Result.Ok();
    }

    public ListCard? GetCardById(Guid cardId) => _cards.FirstOrDefault(c => c.Id == cardId);
    
    private Result UpdateTitle(string title)
    {
        var titleResult = Title.TryFrom(title);
        if(!titleResult.IsSuccess)
            return Result.Fail("TitleInvalid");

        this.Title = titleResult.ValueObject;
        return Result.Ok();
    }

    private Result UpdatePosition(double position)
    {
        if (position < 0)
            return Result.Fail(new Error("PositionMustBeNonNegative").WithMetadata("PropertyName", nameof(Position)));
        
        this.Position = position;
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    private Result UpdateColor(int colorArgb)
    {
        try
        {
            this.ListColor = Color.FromArgb(colorArgb);
            return Result.Ok();
        }
        catch (ArgumentException)
        {
            return Result.Fail(new Error("InvalidColorArgbValue").WithMetadata("PropertyName", nameof(ListColor)));
        }
    }
}