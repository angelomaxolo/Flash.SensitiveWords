using Flash.SensitiveWords.Domain.Entities;

namespace Flash.SensitiveWords.Application.Services;

public interface ISensitiveWordService
{
    Task<IEnumerable<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SensitiveWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SensitiveWord> AddAsync(string word, CancellationToken cancellationToken = default);
    Task<SensitiveWord?> UpdateAsync(Guid id, string newWord, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> FilterMessageAsync(string message, CancellationToken cancellationToken = default);
}
