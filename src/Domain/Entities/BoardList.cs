using System.Drawing;
using System.Text.Json.Serialization;
using Domain.Common;
using FluentResults;

namespace Domain.Entities;

public sealed class BoardList
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public string Title { get; private set; }
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
    
    public static Result<BoardList> Create(Guid BoardId, string Title, int Position)
    {
        var errors = new List<Error>();
        if (BoardId == Guid.Empty)
            errors.Add(new Error("BoardId is required.").WithMetadata("PropertyName", nameof(BoardId)));
        
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        
        if (errors.Count != 0)
            return Result.Fail<BoardList>(errors);
        
        return Result.Ok(new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = BoardId,
            Title = Title,
            Position = Position,
            CreatedAt = DateTime.UtcNow
        });
    }
    
    public Result Update(string title, int position, int colorArgb)
    {
        var errors = new List<IError>();
        
        var titleResult = UpdateTitle(title);
        if (titleResult.IsFailed)
            errors.AddRange(titleResult.Errors);
        
        var positionResult = UpdatePosition(position);
        if (positionResult.IsFailed)
            errors.AddRange(positionResult.Errors);
        
        var colorResult = UpdateColor(colorArgb);
        if (colorResult.IsFailed)
            errors.AddRange(colorResult.Errors);
        
        if (errors.Count != 0)
            return Result.Fail(errors);
        
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Ok();
    }
    
    public Result Patch(PatchValue<string?> title, PatchValue<int?> position, PatchValue<int?> colorArgb)
    {
        var errors = new List<IError>();
        
        if (title.IsSet)
        {
            if(title.Value is null) //TODO is this check necessary? Since UpdateTitle checks anyway if null/empty
            {
                errors.Add(new Error("Title cannot be null.").WithMetadata("PropertyName", nameof(Title)));
            }
            else
            {
                var titleResult = UpdateTitle(title.Value!);
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
    
    private Result UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        
        if (title.Length > 100)
            return Result.Fail(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        
        this.Title = title;
        return Result.Ok();
    }
    
    private Result UpdatePosition(int position)
    {
        if (position < 0)
            return Result.Fail(new Error("Position must be a non-negative integer.").WithMetadata("PropertyName", nameof(Position)));
        
        this.Position = position;
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
    
    
    [JsonConstructor]
    private BoardList(Guid id, Guid boardId, string title, int position, int listColorArgb, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        BoardId = boardId;
        Title = title;
        Position = position;
        ListColorArgb = listColorArgb;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}