using Application.Abstractions.CQRS;
using Application.DTOs.Ollama;

namespace Application.Features.Ollama.Commands;

public sealed record ChatRequestCommand : ICommand<ChatResponse>
{
    public required Guid BoardId { get; init; }
    public required string Message { get; init; }
    public required Guid RequestingUserId { get; init; }
}