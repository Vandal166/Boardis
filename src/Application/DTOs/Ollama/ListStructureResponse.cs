namespace Application.DTOs.Ollama;

public record KanbanCard(string Title, string? Description = null);  // Description optional

public record ListStructure(string Name, List<KanbanCard> Cards);  // Cards can be empty

public sealed record ListStructureResponse(List<ListStructure> Lists);