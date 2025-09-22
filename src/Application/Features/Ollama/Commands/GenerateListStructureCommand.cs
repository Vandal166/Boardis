using Application.Abstractions.CQRS;
using Application.DTOs.Ollama;

namespace Application.Features.Ollama.Commands;

public sealed class GenerateListStructureCommand : ICommand<ListStructureResponse>
{
    public required string Description { get; init; }
    public required Guid RequestingUserId { get; init; }
}