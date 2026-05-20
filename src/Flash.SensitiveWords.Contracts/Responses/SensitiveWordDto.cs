namespace Flash.SensitiveWords.Contracts.Responses
{
    public class SensitiveWordDto
    {
        public Guid Id { get; set; }
        public string Word { get; set; } = string.Empty;
    }
}
