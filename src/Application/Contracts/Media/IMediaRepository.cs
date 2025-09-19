namespace Application.Contracts.Media;

using Media = Domain.Images.Entities.Media;

public interface IMediaRepository
{
    Task AddAsync(Media media, CancellationToken ct = default);
    Task DeleteAsync(Media media, CancellationToken ct = default);
    
    Task<Media?> GetByIdAsync(Guid mediaId, CancellationToken ct = default);
    Task<List<Media>?> GetByEntityIdAsync(Guid boundToId, CancellationToken ct = default);
}