namespace Application.DTOs.Ollama;

public sealed record ChatResponse
{
    public required string ResponseMessage { get; init; }
}