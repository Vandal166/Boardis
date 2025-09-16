using System.Drawing;
using Domain.Common;
using Domain.ValueObjects;
using FluentResults;

namespace Domain.Entities;

public sealed class BoardList : Entity
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Title Title { get; private set; }
    public int Position { get; private set; } // position/order of the list on the board
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
    
    internal static Result<BoardList> Create(Guid BoardId, Title Title, int Position)
    {
        if (BoardId == Guid.Empty)
            return Result.Fail<BoardList>(new Error("BoardId is required.").WithMetadata("PropertyName", nameof(BoardId)));
        
        return Result.Ok(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = BoardId,
            Title = Title,
            Position = Position,
            CreatedAt = DateTime.UtcNow
        });
    }
    public Result Patch(PatchValue<string?> title, PatchValue<int?> position, PatchValue<int?> colorArgb)
    {
        var errors = new List<IError>();
        
        if (title.IsSet)
        {
            if(title.Value is null)
            {
                errors.Add(new Error("Title cannot be null.").WithMetadata("PropertyName", nameof(Title)));
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
    
    public Result<ListCard> AddCard(Title title, string? description, double position)
    {
        var cardResult = ListCard.Create(Id, title, description, position);
        if (cardResult.IsFailed)
            return Result.Fail<ListCard>(cardResult.Errors);
        
        _cards.Add(cardResult.Value);
        UpdatedAt = DateTime.UtcNow;
        return cardResult;
    }

    public Result RemoveCard(Guid cardId)
    {
        var card = _cards.FirstOrDefault(c => c.Id == cardId);
        if (card is null)
            return Result.Fail(new Error("Card not found."));

        _cards.Remove(card);
        UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }

    public ListCard? GetCardById(Guid cardId) => _cards.FirstOrDefault(c => c.Id == cardId);
    
    private Result UpdateTitle(string title)
    {
        var titleResult = Title.TryFrom(title);
        if(!titleResult.IsSuccess)
            return Result.Fail(titleResult.Error.ErrorMessage);

        this.Title = titleResult.ValueObject;
        return Result.Ok();
    }

    private Result UpdatePosition(int position)
    {
        if (position < 0)
            return Result.Fail(new Error("Position must be a non-negative integer.").WithMetadata("PropertyName", nameof(Position)));
        
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
            return Result.Fail(new Error("Invalid color ARGB value.").WithMetadata("PropertyName", nameof(ListColor)));
        }
    }
}