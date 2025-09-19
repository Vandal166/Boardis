using Application.Abstractions.CQRS;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Media.Commands;

public sealed record UploadMediaCommand : ICommand<Domain.Images.Entities.Media>
{
    public required Guid EntityId { get; init; }
    public required IFormFile File { get; init; }
    public required Guid RequestingUserId { get; init; }
}