using Application.Abstractions.CQRS;
using Application.DTOs.Media;

namespace Application.Features.Media.Queries;

public sealed record GetMediaByIdQuery : IQuery<MediaResponse>, ICacheableQuery
{
    public required Guid MediaId { get; init; }
    public required Guid RequestingUserId { get; init; }
    
    public string CacheKey => $"media_{MediaId}";
    public bool BypassCache => false;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration { get; } = TimeSpan.FromMinutes(2);
}