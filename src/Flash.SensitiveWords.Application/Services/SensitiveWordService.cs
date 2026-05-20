using Flash.SensitiveWords.Application.Exceptions;
using Flash.SensitiveWords.Application.Models;
using Flash.SensitiveWords.Domain.Entities;
using Flash.SensitiveWords.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Flash.SensitiveWords.Application.Services;

/// <summary>
/// Service for managing sensitive words and filtering messages.
/// Implements business logic with proper exception handling and validation.
/// </summary>
public sealed class SensitiveWordService : ISensitiveWordService
{
    private readonly ISensitiveWordRepository _repository;
    private readonly ILogger<SensitiveWordService>? _logger;

    public SensitiveWordService(
        ISensitiveWordRepository repository,
        ILogger<SensitiveWordService>? logger = null)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<SensitiveWordResult>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting retrieval of all sensitive words.");

        var entities = await _repository.GetAllAsync(cancellationToken);
        var result = entities.Select(MapToResult).ToList();
        var count = result.Count;

        _logger?.LogInformation("Retrieved {Count} sensitive words.", count);
        return result;
    }

    public async Task<SensitiveWordResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Retrieving sensitive word for Id={Id}.", id);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger?.LogWarning("Sensitive word with Id={Id} not found.", id);
            throw new SensitiveWordNotFoundException(id);
        }

        return MapToResult(entity);
    }

    public async Task<SensitiveWordResult> AddAsync(string word, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Adding a new sensitive word.");

        if (string.IsNullOrWhiteSpace(word))
        {
            _logger?.LogWarning("Attempted to add an empty sensitive word.");
            throw new ArgumentException("Word cannot be empty.", nameof(word));
        }

        if (await _repository.ExistsAsync(word, cancellationToken))
        {
            _logger?.LogWarning("Sensitive word already exists for length {WordLength}.", word.Length);
            throw new SensitiveWordAlreadyExistsException(word);
        }

        var entity = new SensitiveWord(word);
        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation("Sensitive word added successfully with Id={Id} and length {WordLength}.", entity.Id, entity.Word.Length);
        return MapToResult(entity);
    }

    public async Task<SensitiveWordResult> UpdateAsync(Guid id, string newWord, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Updating sensitive word Id={Id}.", id);

        if (string.IsNullOrWhiteSpace(newWord))
        {
            _logger?.LogWarning("Attempted to update sensitive word Id={Id} with an empty value.", id);
            throw new ArgumentException("Word cannot be empty.", nameof(newWord));
        }

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger?.LogWarning("Sensitive word Id={Id} not found for update.", id);
            throw new SensitiveWordNotFoundException(id);
        }

        newWord = newWord.Trim();
        if (existing.Word.Equals(newWord, StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogDebug("No update needed for sensitive word Id={Id} because the value is unchanged.", id);
            return MapToResult(existing);
        }

        if (await _repository.ExistsAsync(newWord, cancellationToken))
        {
            _logger?.LogWarning("Sensitive word already exists and cannot be used for update. length={WordLength}.", newWord.Length);
            throw new SensitiveWordAlreadyExistsException(newWord);
        }

        existing.UpdateWord(newWord);
        await _repository.UpdateAsync(existing, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger?.LogInformation("Sensitive word Id={Id} updated successfully with new length {WordLength}.", id, newWord.Length);
        return MapToResult(existing);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Deleting sensitive word Id={Id}.", id);

        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger?.LogWarning("Sensitive word Id={Id} not found for deletion.", id);
            throw new SensitiveWordNotFoundException(id);
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (deleted)
        {
            await _repository.SaveChangesAsync(cancellationToken);
            _logger?.LogInformation("Sensitive word Id={Id} deleted successfully.", id);
        }
        else
        {
            _logger?.LogWarning("DeleteAsync reported no entity removed for Id={Id}.", id);
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

        var filterResult = new FilterMessageResult
        {
            OriginalMessage = message,
            FilteredMessage = result,
            WordsFiltered = wordsFiltered
        };

        _logger?.LogInformation("Filtered message with {WordsFiltered} sensitive words replaced.", wordsFiltered);
        return filterResult;
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
