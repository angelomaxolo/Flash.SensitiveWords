namespace Flash.SensitiveWords.Contracts.Requests
{
    /// <summary>
    /// Request model for creating a new sensitive word.
    /// </summary>
    public class CreateSensitiveWordRequest
    {
        /// <summary>
        /// The sensitive word text to add.
        /// </summary>
        public string Word { get; set; } = string.Empty;
    }
}
