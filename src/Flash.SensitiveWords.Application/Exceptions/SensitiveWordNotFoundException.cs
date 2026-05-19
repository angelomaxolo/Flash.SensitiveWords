namespace Flash.SensitiveWords.Application.Exceptions;

/// <summary>
/// Thrown when a sensitive word cannot be found by ID.
/// </summary>
public sealed class SensitiveWordNotFoundException : ApplicationException
{
    public Guid Id { get; }

    public SensitiveWordNotFoundException(Guid id) 
        : base($"Sensitive word with ID '{id}' was not found.")
    {
        Id = id;
    }
}
