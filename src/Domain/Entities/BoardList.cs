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
    
    public IReadOnlyCollection<Card> Cards => _cards.AsReadOnly();
    private readonly List<Card> _cards = new();
    
    private BoardList() { }
    
    public static Result<BoardList> Create(Guid boardId, string title)
    {
        if (boardId == Guid.Empty)
            return Result.Fail<BoardList>("Board ID cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(title))
            return Result.Fail<BoardList>("Title cannot be empty.");
        
        if (title.Length > 100)
            return Result.Fail<BoardList>("Title cannot exceed 100 characters.");
        
        var boardList = new BoardList
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            Title = title,
            CreatedAt = DateTime.UtcNow
        };
        
        return Result.Ok(boardList);
    }
    
    public Result UpdateColor(Color newColor)
    {
        // todo role check etc
        ListColor = newColor;
        UpdatedAt = DateTime.UtcNow;
        
        return Result.Ok();
    }
}