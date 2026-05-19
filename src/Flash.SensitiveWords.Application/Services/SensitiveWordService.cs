using Flash.SensitiveWords.Application.Exceptions;
using Flash.SensitiveWords.Application.Models;
using Flash.SensitiveWords.Domain.Entities;
using Flash.SensitiveWords.Domain.Repositories;
using System.Text.RegularExpressions;

namespace Flash.SensitiveWords.Application.Services;

/// <summary>
/// Service for managing sensitive words and filtering messages.
/// Implements business logic with proper exception handling and validation.
/// </summary>
public sealed class SensitiveWordService : ISensitiveWordService
{
    private readonly ISensitiveWordRepository _repository;

    public SensitiveWordService(ISensitiveWordRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SensitiveWordResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToResult);
    }

    public async Task<SensitiveWordResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            throw new SensitiveWordNotFoundException(id);
        }

        return MapToResult(entity);
    }

    public async Task<SensitiveWordResult> AddAsync(string word, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ArgumentException("Word cannot be empty.", nameof(word));
        }

        if (await _repository.ExistsAsync(word, cancellationToken))
        {
            throw new SensitiveWordAlreadyExistsException(word);
        }

        var entity = new SensitiveWord(word);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapToResult(entity);
    }

    public async Task<SensitiveWordResult> UpdateAsync(Guid id, string newWord, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(newWord))
        {
            throw new ArgumentException("Word cannot be empty.", nameof(newWord));
        }

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new SensitiveWordNotFoundException(id);
        }

        newWord = newWord.Trim();
        if (existing.Word.Equals(newWord, StringComparison.OrdinalIgnoreCase))
        {
            return MapToResult(existing);
        }

        if (await _repository.ExistsAsync(newWord, cancellationToken))
        {
            throw new SensitiveWordAlreadyExistsException(newWord);
        }

        existing.UpdateWord(newWord);
        await _repository.UpdateAsync(existing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapToResult(existing);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            throw new SensitiveWordNotFoundException(id);
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<FilterMessageResult> FilterMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty.", nameof(message));
        }

        var sensitiveWords = await _repository.GetAllAsync(cancellationToken);

        if (!sensitiveWords.Any())
        {
            return new FilterMessageResult
            {
                OriginalMessage = message,
                FilteredMessage = message,
                WordsFiltered = 0
            };
        }

        var result = message;
        int wordsFiltered = 0;

        foreach (var sensitiveWord in sensitiveWords)
        {
            var pattern = $@"\b{Regex.Escape(sensitiveWord.Word)}\b";
            var replacement = new string('*', sensitiveWord.Word.Length);
            var matches = Regex.Matches(result, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                wordsFiltered += matches.Count;
                result = Regex.Replace(result, pattern, replacement, RegexOptions.IgnoreCase);
            }
        }

        return new FilterMessageResult
        {
            OriginalMessage = message,
            FilteredMessage = result,
            WordsFiltered = wordsFiltered
        };
    }

    private static SensitiveWordResult MapToResult(SensitiveWord entity)
    {
        return new SensitiveWordResult
        {
            Id = entity.Id,
            Word = entity.Word,
            CreatedDate = entity.CreatedDate
        };
    }
}
