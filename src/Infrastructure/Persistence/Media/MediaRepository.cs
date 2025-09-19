using Application.Contracts.Media;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Media;

using Media = Domain.Images.Entities.Media;

internal sealed class MediaRepository : IMediaRepository
{
    private readonly BoardisDbContext _dbContext;
    public MediaRepository(BoardisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Media media, CancellationToken ct = default)
    {
        await _dbContext.Media.AddAsync(media, ct);
    }

    public async Task<Media?> GetByIdAsync(Guid mediaId, CancellationToken ct = default)
    {
        return await _dbContext.Media
            .FirstOrDefaultAsync(m => m.Id == mediaId, ct);
    }

    public async Task<List<Media>?> GetByEntityIdAsync(Guid boundToId, CancellationToken ct = default)
    {
        return await _dbContext.Media
            .Where(m => m.BoundToEntityId == boundToId)
            .ToListAsync(ct);
    }
}