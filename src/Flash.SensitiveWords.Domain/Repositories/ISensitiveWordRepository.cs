using Flash.SensitiveWords.Domain.Entities;

namespace Flash.SensitiveWords.Domain.Repositories;

public interface ISensitiveWordRepository
{
    Task<IEnumerable<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SensitiveWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string word, CancellationToken cancellationToken = default);
    Task AddAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default);
    Task UpdateAsync(SensitiveWord sensitiveWord, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
