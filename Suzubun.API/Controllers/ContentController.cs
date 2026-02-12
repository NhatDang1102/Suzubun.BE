using Microsoft.AspNetCore.Mvc;
using Suzubun.Service.Models;
using Suzubun.Service.Services;

namespace Suzubun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly IContentService _contentService;
    private readonly IDictionaryService _dictionaryService;
    private readonly IJapaneseService _japaneseService;

    public ContentController(IContentService contentService, IDictionaryService dictionaryService, IJapaneseService japaneseService)
    {
        _contentService = contentService;
        _dictionaryService = dictionaryService;
        _japaneseService = japaneseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PaginationParams @params, [FromQuery] string? type, [FromQuery] Guid? categoryId)
    {
        var result = await _contentService.GetPagedContentsAsync(@params, type, categoryId, onlyPublished: true);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var content = await _contentService.GetDetailAsync(id);
        if (content == null || !content.IsPublished) return NotFound();

        var lines = await _contentService.GetLinesAsync(id);
        return Ok(new { Content = content, Lines = lines });
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string word, [FromQuery] string context)
    {
        var result = await _dictionaryService.LookUpAsync(word, context);
        return Ok(result);
    }

    [HttpPost("translate")]
    public async Task<IActionResult> Translate([FromBody] TranslateRequest request)
    {
        if (string.IsNullOrEmpty(request.Sentence)) return BadRequest("Sentence is required.");
        var translation = await _japaneseService.TranslateSentenceAsync(request.Sentence);
        return Ok(new { Translation = translation });
    }
}

public class TranslateRequest
{
    public string Sentence { get; set; } = string.Empty;
}
