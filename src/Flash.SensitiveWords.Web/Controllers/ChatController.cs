using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flash.SensitiveWords.Web.Controllers;

public class ChatController : Controller
{
    private readonly IMessageFilterClient _client;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IMessageFilterClient client, ILogger<ChatController> logger)
    {
        _client = client;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new ChatViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(ChatViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Message))
        {
            _logger.LogWarning("Chat filter request rejected because message was empty.");
            ModelState.AddModelError("", "Message cannot be empty");
            return View(model);
        }

        _logger.LogInformation("Chat filter request received for message length {MessageLength}.", model.Message.Length);
        var response = await _client.FilterAsync(new FilterMessageRequest
        {
            Message = model.Message
        });

        if (response is not null && response.Success && response.Result is not null)
        {
            model.FilteredMessage = response.Result.FilteredMessage;
            _logger.LogInformation("Chat filter completed successfully.");
        }
        else
        {
            model.FilteredMessage = "Service unavailable. Please try again.";
            _logger.LogWarning("Chat filter request failed or returned an unsuccessful response.");
        }

        return View(model);
    }
}