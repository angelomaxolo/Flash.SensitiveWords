namespace Flash.SensitiveWords.Application.Exceptions;

/// <summary>
/// Thrown when attempting to create a sensitive word that already exists.
/// </summary>
public sealed class SensitiveWordAlreadyExistsException : ApplicationException
{
    public SensitiveWordAlreadyExistsException(string word) 
        : base($"A sensitive word with the value '{word}' already exists.") { }
}
