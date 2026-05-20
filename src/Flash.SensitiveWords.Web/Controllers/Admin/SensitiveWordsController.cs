using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Flash.SensitiveWords.Web.Controllers.Admin;

[Route("sensitivewords")]
[Authorize]
public class SensitiveWordsController : Controller
{
    private readonly ISensitiveWordsClient _client;
    private readonly ILogger<SensitiveWordsController> _logger;

    public SensitiveWordsController(ISensitiveWordsClient client, ILogger<SensitiveWordsController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("Loading admin sensitive words dashboard.");

        var response = await _client.GetAllAsync();
        var model = response?.Result?
            .Select(x => new SensitiveWordViewModel
            {
                Id = x.Id,
                Word = x.Word
            }).ToList() ?? new List<SensitiveWordViewModel>();

        _logger.LogInformation("Loaded {Count} sensitive words for admin view.", model.Count);
        return View(model);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(string word)
    {
        _logger.LogInformation("Admin create requested for a sensitive word of length {WordLength}.", word?.Length ?? 0);

        if (string.IsNullOrWhiteSpace(word))
        {
            _logger.LogWarning("Admin create rejected because word was empty.");
            return BadRequest();
        }

        var response = await _client.CreateAsync(new CreateSensitiveWordRequest
        {
            Word = word
        });

        _logger.LogInformation("Admin create completed for word length {WordLength}.", word.Length);
        return Json(new
        {
            id = response?.Result?.Id,
            word = response?.Result?.Word
        });
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Admin delete requested for sensitive word Id={Id}.", id);
        await _client.DeleteAsync(id);
        _logger.LogInformation("Admin delete completed for sensitive word Id={Id}.", id);
        return Ok();
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateSensitiveWordRequest request)
    {
        _logger.LogInformation("Admin update requested for sensitive word Id={Id}.", request.Id);

        if (string.IsNullOrWhiteSpace(request.Word))
        {
            _logger.LogWarning("Admin update rejected because request word was empty for Id={Id}.", request.Id);
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Word cannot be empty"
            });
        }

        var updated = await _client.UpdateAsync(request);
        _logger.LogInformation("Admin update completed for sensitive word Id={Id}.", request.Id);

        return Ok(new ApiResponse<SensitiveWordDto>
        {
            Success = true,
            Message = "Updated successfully",
            Result = updated?.Result
        });
    }
}