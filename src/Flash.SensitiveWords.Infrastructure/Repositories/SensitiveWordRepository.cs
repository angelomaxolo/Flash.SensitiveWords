using Flash.SensitiveWords.Domain.Entities;
using Flash.SensitiveWords.Domain.Repositories;
using Flash.SensitiveWords.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Flash.SensitiveWords.Infrastructure.Repositories;

public sealed class SensitiveWordRepository : ISensitiveWordRepository
{
    private readonly SensitiveWordsDbContext _dbContext;

    public SensitiveWordRepository(SensitiveWordsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensitiveWords
            .AsNoTracking()
            .OrderBy(word => word.Word)
            .ToListAsync(cancellationToken);
    }

    public async Task<SensitiveWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensitiveWords
            .AsNoTracking()
            .FirstOrDefaultAsync(word => word.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string word, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SensitiveWords
            .AnyAsync(entity => entity.Word == word.Trim(), cancellationToken);
    }

    public async Task AddAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default)
    {
        await _dbContext.SensitiveWords.AddAsync(sensitiveWord, cancellationToken);
    }

    public Task UpdateAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default)
    {
        _dbContext.SensitiveWords.Update(sensitiveWord);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.SensitiveWords.FirstOrDefaultAsync(
            w => w.Id == id, cancellationToken);

        if (entity is not null)
        {
            _dbContext.SensitiveWords.Remove(entity);
            return true;
        }

        return false;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
