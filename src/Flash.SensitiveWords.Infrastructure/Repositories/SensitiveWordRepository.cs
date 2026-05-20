using Flash.SensitiveWords.Domain.Entities;
using Flash.SensitiveWords.Domain.Repositories;
using Flash.SensitiveWords.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Flash.SensitiveWords.Infrastructure.Repositories;

public sealed class SensitiveWordRepository : ISensitiveWordRepository
{
    private readonly SensitiveWordsDbContext _dbContext;
    private readonly ILogger<SensitiveWordRepository> _logger;

    public SensitiveWordRepository(SensitiveWordsDbContext dbContext, ILogger<SensitiveWordRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IEnumerable<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving all sensitive words from the database.");

        return await _dbContext.SensitiveWords
            .AsNoTracking()
            .OrderBy(word => word.Word)
            .ToListAsync(cancellationToken);
    }

    public async Task<SensitiveWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving sensitive word with Id={Id}.", id);

        return await _dbContext.SensitiveWords
            .AsNoTracking()
            .FirstOrDefaultAsync(word => word.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string word, CancellationToken cancellationToken = default)
    {
        var trimmedWord = word.Trim();
        _logger.LogDebug("Checking existence for sensitive word of length {Length}.", trimmedWord.Length);

        return await _dbContext.SensitiveWords
            .AnyAsync(entity => entity.Word == trimmedWord, cancellationToken);
    }

    public async Task AddAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new sensitive word to the database with length {WordLength}.", sensitiveWord.Word.Length);
        await _dbContext.SensitiveWords.AddAsync(sensitiveWord, cancellationToken);
    }

    public Task UpdateAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating sensitive word Id={Id} with new length {WordLength}.", sensitiveWord.Id, sensitiveWord.Word.Length);
        _dbContext.SensitiveWords.Update(sensitiveWord);
        return Task.CompletedTask;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting sensitive word with Id={Id}.", id);

        var entity = await _dbContext.SensitiveWords.FirstOrDefaultAsync(
            w => w.Id == id, cancellationToken);

        if (entity is not null)
        {
            _dbContext.SensitiveWords.Remove(entity);
            return true;
        }

        _logger.LogWarning("Sensitive word with Id={Id} was not found for deletion.", id);
        return false;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Committing changes to the database.");
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
