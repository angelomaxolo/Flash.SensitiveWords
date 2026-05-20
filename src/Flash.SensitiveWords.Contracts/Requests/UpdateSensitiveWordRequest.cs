namespace Flash.SensitiveWords.Contracts.Requests
{
    public class UpdateSensitiveWordRequest
    {
        public Guid Id { get; set; }
        public string Word { get; set; } = string.Empty;
    }
}
