namespace Flash.SensitiveWords.Application.Exceptions;

/// <summary>
/// Base exception for application-layer business logic errors.
/// </summary>
public class ApplicationException : Exception
{
    public ApplicationException(string message) : base(message) { }

    public ApplicationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
