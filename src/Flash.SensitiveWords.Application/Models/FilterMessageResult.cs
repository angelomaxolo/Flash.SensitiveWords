namespace Flash.SensitiveWords.Application.Models;

/// <summary>
/// Represents the result of filtering a message in the Application layer.
/// This is an internal Application model, not exposed via API contracts.
/// </summary>
public sealed class FilterMessageResult
{
    /// <summary>The original unfiltered message.</summary>
    public string OriginalMessage { get; set; } = string.Empty;

    /// <summary>The message after sensitive words have been filtered.</summary>
    public string FilteredMessage { get; set; } = string.Empty;

    /// <summary>The number of sensitive words found and replaced.</summary>
    public int WordsFiltered { get; set; }
}
