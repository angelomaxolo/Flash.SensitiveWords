using Flash.SensitiveWords.Domain.Entities;
using Flash.SensitiveWords.Domain.Repositories;
using System.Text.RegularExpressions;

namespace Flash.SensitiveWords.Application.Services;

public sealed class SensitiveWordService : ISensitiveWordService
{
    private readonly ISensitiveWordRepository _repository;

    public SensitiveWordService(ISensitiveWordRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SensitiveWord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<SensitiveWord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<SensitiveWord> AddAsync(string word, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsAsync(word, cancellationToken))
        {
            throw new InvalidOperationException("This sensitive word already exists.");
        }

        var entity = new SensitiveWord(word);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<SensitiveWord?> UpdateAsync(Guid id, string newWord, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newWord))
        {
            throw new ArgumentException("Word cannot be empty.", nameof(newWord));
        }

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        newWord = newWord.Trim();
        if (existing.Word.Equals(newWord, StringComparison.OrdinalIgnoreCase))
        {
            return existing;
        }

        if (await _repository.ExistsAsync(newWord, cancellationToken))
        {
            throw new InvalidOperationException("This sensitive word already exists.");
        }

        existing.UpdateWord(newWord);
        await _repository.UpdateAsync(existing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        return deleted;
    }

    public async Task<string> FilterMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        var sensitiveWords = await _repository.GetAllAsync(cancellationToken);

        if (!sensitiveWords.Any())
        {
            return message;
        }

        var result = message;
        foreach (var sensitiveWord in sensitiveWords)
        {
            var pattern = $@"\b{Regex.Escape(sensitiveWord.Word)}\b";
            var replacement = new string('*', sensitiveWord.Word.Length);
            result = Regex.Replace(result, pattern, replacement, RegexOptions.IgnoreCase);
        }

        return result;
    }
}
