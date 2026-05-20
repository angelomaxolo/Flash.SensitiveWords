namespace Flash.SensitiveWords.Application.Models;

/// <summary>
/// Represents the result of a sensitive word operation in the Application layer.
/// This is an internal Application model, not exposed via API contracts.
/// </summary>
public sealed class SensitiveWordResult
{
    /// <summary>The unique identifier of the sensitive word.</summary>
    public Guid Id { get; set; }

    /// <summary>The sensitive word text.</summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>The date and time when the word was created.</summary>
    public DateTime CreatedDate { get; set; }
}
