using System.Drawing;
using FluentResults;

namespace Domain.Entities;

public sealed class BoardList
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public string Title { get; private set; }
    public int Position { get; private set; } // position/order of the list on the board
    private int _listColorArgb = Color.LightBlue.ToArgb();
    public Color ListColor
    {
        get => Color.FromArgb(_listColorArgb);
        private set => _listColorArgb = value.ToArgb();
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
    
    public Result Update(string Title, int Position, int ColorArgb)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(new Error("Title is required.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Title.Length > 100)
            errors.Add(new Error("Title cannot exceed 100 characters.").WithMetadata("PropertyName", nameof(Title)));
        
        if (Position < 0)
            errors.Add(new Error("Position must be a non-negative integer.").WithMetadata("PropertyName", nameof(Position)));
        
        if (errors.Count != 0)
            return Result.Fail(errors);
        
        this.Title = Title;
        this.Position = Position;
        this.ListColor = Color.FromArgb(ColorArgb);
        this.UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
    
    
    public Result UpdateColor(Color newColor)
    {
        // todo role check etc
        ListColor = newColor;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
}