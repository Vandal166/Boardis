using Application.Abstractions.CQRS;
using Application.DTOs.Media;

namespace Application.Features.Media.Queries;

public sealed record GetMediaByIdQuery : IQuery<List<MediaResponse>>
{
    public required Guid EntityId { get; init; }
    public required Guid RequestingUserId { get; init; }
}