using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using Flash.SensitiveWords.RestClient.Abstractions;
using Flash.SensitiveWords.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Flash.SensitiveWords.Web.Controllers.Admin;

[Route("sensitivewords")]
public class SensitiveWordsController : Controller
{
    private readonly ISensitiveWordsClient _client;

    public SensitiveWordsController(ISensitiveWordsClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var response = await _client.GetAllAsync();

        var model = response?.Result?
            .Select(x => new SensitiveWordViewModel
            {
                Id = x.Id,
                Word = x.Word
            }).ToList() ?? new List<SensitiveWordViewModel>();

        return View(model);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return BadRequest();

        var response = await _client.CreateAsync(new CreateSensitiveWordRequest
        {
            Word = word
        });

        return Json(new
        {
            id = response?.Result?.Id,
            word = response?.Result?.Word
        });
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _client.DeleteAsync(id);
        return Ok();
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UpdateSensitiveWordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Word))
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Word cannot be empty"
            });
        }

       var updated = await _client.UpdateAsync(request);

        return Ok(new ApiResponse<SensitiveWordDto>
        {
            Success = true,
            Message = "Updated successfully",
            Result = updated?.Result
        });
    }
}