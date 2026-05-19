using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Flash.SensitiveWords.Web.Controllers;

public class ChatController : Controller
{
    private readonly IMessageFilterClient _client;

    public ChatController(IMessageFilterClient client)
    {
        _client = client;
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
            ModelState.AddModelError("", "Message cannot be empty");
            return View(model);
        }

        var response = await _client.FilterAsync(new FilterMessageRequest
        {
            Message = model.Message
        });

        if (response is not null && response.Success && response.Result is not null)
        {
            model.FilteredMessage = response.Result.FilteredMessage;
        }
        else
        {
            model.FilteredMessage = "Service unavailable. Please try again.";
        }

        return View(model);
    }
}