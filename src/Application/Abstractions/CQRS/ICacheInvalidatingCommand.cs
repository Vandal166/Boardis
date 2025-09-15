namespace Application.Abstractions.CQRS;

public interface ICacheInvalidatingCommand
{
    IEnumerable<string> CacheKeysToInvalidate { get; }  // Return affected keys (e.g., $"cards_{BoardListId}")
}