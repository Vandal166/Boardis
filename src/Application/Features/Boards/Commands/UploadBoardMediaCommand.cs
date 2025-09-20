using Application.Abstractions.CQRS;
using Domain.Board.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Boards.Commands;

public sealed record UploadBoardMediaCommand : ICommand<Board>
{
    public required Guid BoardId { get; init; }
    public required IFormFile File { get; init; }
    public required Guid RequestingUserId { get; init; }
}