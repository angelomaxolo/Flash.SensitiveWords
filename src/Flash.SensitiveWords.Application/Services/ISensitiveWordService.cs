using Flash.SensitiveWords.Application.Models;

namespace Flash.SensitiveWords.Application.Services;

/// <summary>
/// Service interface for managing sensitive words and filtering messages.
/// Returns Application-layer result models, not API contracts.
/// </summary>
public interface ISensitiveWordService
{
    /// <summary>
    /// Retrieves all sensitive words.
    /// </summary>
    Task<IEnumerable<SensitiveWordResult>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sensitive word by its unique identifier.
    /// </summary>
    /// <exception cref="SensitiveWordNotFoundException">Thrown if the word is not found.</exception>
    Task<SensitiveWordResult> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new sensitive word.
    /// </summary>
    /// <exception cref="SensitiveWordAlreadyExistsException">Thrown if the word already exists.</exception>
    /// <exception cref="ArgumentException">Thrown if the word is null or whitespace.</exception>
    Task<SensitiveWordResult> AddAsync(string word, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sensitive word.
    /// </summary>
    /// <exception cref="SensitiveWordNotFoundException">Thrown if the word is not found.</exception>
    /// <exception cref="SensitiveWordAlreadyExistsException">Thrown if the new word already exists.</exception>
    /// <exception cref="ArgumentException">Thrown if the new word is null or whitespace.</exception>
    Task<SensitiveWordResult> UpdateAsync(Guid id, string newWord, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sensitive word by its unique identifier.
    /// </summary>
    /// <exception cref="SensitiveWordNotFoundException">Thrown if the word is not found.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters a message by replacing sensitive words with asterisks.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the message is null or whitespace.</exception>
    Task<FilterMessageResult> FilterMessageAsync(string message, CancellationToken cancellationToken = default);
}
