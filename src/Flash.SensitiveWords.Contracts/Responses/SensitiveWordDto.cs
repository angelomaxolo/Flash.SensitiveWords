namespace Flash.SensitiveWords.Contracts.Responses
{
    /// <summary>
    /// Represents a sensitive word entry returned by the API.
    /// </summary>
    public class SensitiveWordDto
    {
        /// <summary>
        /// The unique identifier of the sensitive word.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The sensitive word value.
        /// </summary>
        public string Word { get; set; } = string.Empty;
    }
}
