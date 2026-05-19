namespace Flash.SensitiveWords.Contracts.Responses
{
    /// <summary>
    /// Represents error details in an API response.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>The error code/type.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>The detailed error message.</summary>
        public string Message { get; set; } = string.Empty;
    }
}
