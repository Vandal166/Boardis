namespace Application.DTOs.Ollama;

public sealed class ChatRequest
{
    public required Guid BoardId { get; init; }
    public required string Message { get; init; }
}