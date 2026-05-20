namespace Flash.SensitiveWords.Contracts.Responses
{
    /// <summary>
    /// Response model returned after filtering a message.
    /// </summary>
    public class FilterMessageResponse
    {
        /// <summary>
        /// The original message before filtering.
        /// </summary>
        public string OriginalMessage { get; set; } = string.Empty;

        /// <summary>
        /// The message after sensitive words have been masked.
        /// </summary>
        public string FilteredMessage { get; set; } = string.Empty;
    }
}
