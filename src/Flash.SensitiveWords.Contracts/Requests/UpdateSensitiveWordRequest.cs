namespace Flash.SensitiveWords.Contracts.Requests
{
    /// <summary>
    /// Request model for updating an existing sensitive word.
    /// </summary>
    public class UpdateSensitiveWordRequest
    {
        /// <summary>
        /// The unique identifier of the sensitive word to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The new sensitive word text.
        /// </summary>
        public string Word { get; set; } = string.Empty;
    }
}
