namespace Flash.SensitiveWords.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Result { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
