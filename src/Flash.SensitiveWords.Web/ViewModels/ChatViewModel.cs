namespace Flash.SensitiveWords.Web.ViewModels
{
    public class ChatViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string? FilteredMessage { get; set; }
    }
}
