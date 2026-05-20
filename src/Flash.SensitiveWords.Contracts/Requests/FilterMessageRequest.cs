namespace Flash.SensitiveWords.Contracts.Requests
{
    /// <summary>
    /// Request model used to filter sensitive words from a message.
    /// </summary>
    public class FilterMessageRequest
    {
        /// <summary>
        /// The chat message text to filter.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
