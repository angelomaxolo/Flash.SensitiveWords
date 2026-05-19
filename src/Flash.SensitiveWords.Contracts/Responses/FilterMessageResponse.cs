namespace Flash.SensitiveWords.Contracts.Responses
{
    public class FilterMessageResponse
    {
        public string OriginalMessage { get; set; } = string.Empty;
        public string FilteredMessage { get; set; } = string.Empty;
    }
}
