using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Suzubun.Repository.Entities;
using Suzubun.Service.Helpers;
using Suzubun.Service.Models;
using Suzubun.Service.Services;
using System.Security.Claims;

namespace Suzubun.API.Controllers;

[Authorize]
[ApiController]
[Route("api/admin/[controller]")]
public class ContentManagerController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly IContentService _contentService;
    private readonly IJapaneseService _japaneseService;
    private readonly Supabase.Client _adminSupabase;

    public ContentManagerController(IStorageService storageService, IContentService contentService, IJapaneseService japaneseService, [FromKeyedServices("AdminClient")] Supabase.Client adminSupabase)
    {
        _storageService = storageService;
        _contentService = contentService;
        _japaneseService = japaneseService;
        _adminSupabase = adminSupabase;
    }

    private async Task<bool> IsAdmin()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return false;

        var profile = await _adminSupabase.From<Profile>().Where(x => x.Id == Guid.Parse(userId)).Single();
        return profile?.Role == "admin";
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PaginationParams @params, [FromQuery] string? type, [FromQuery] Guid? categoryId)
    {
        if (!await IsAdmin()) return Forbid();
        var result = await _contentService.GetPagedContentsAsync(@params, type, categoryId, onlyPublished: null);
        return Ok(result);
    }

    [HttpPost("upload-article")]
    public async Task<IActionResult> UploadArticle([FromForm] ArticleUploadRequest request)
    {
        if (!await IsAdmin()) return Forbid();

        string? thumbnailUrl = null;
        if (request.Thumbnail != null)
        {
            thumbnailUrl = await _storageService.UploadFileAsync(request.Thumbnail.OpenReadStream(), request.Thumbnail.FileName, "contents/images");
        }

        string? audioUrl = null;
        if (request.Audio != null)
        {
            audioUrl = await _storageService.UploadFileAsync(request.Audio.OpenReadStream(), request.Audio.FileName, "contents/audio");
        }
        else if (!string.IsNullOrEmpty(request.Body))
        {
            // TỰ ĐỘNG SINH GIỌNG ĐỌC NẾU KHÔNG CÓ FILE AUDIO
            try 
            {
                var audioStream = await _japaneseService.GenerateSpeechAsync(request.Body);
                audioUrl = await _storageService.UploadFileAsync(audioStream, "tts_voice.mp3", "contents/audio");
            }
            catch (Exception ex)
            {
                // Nếu lỗi TTS thì cứ bỏ qua để bài báo vẫn đăng được
                Console.WriteLine("TTS Error: " + ex.Message);
            }
        }

        var content = new Content
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            ThumbnailUrl = thumbnailUrl,
            AudioUrl = audioUrl,
            ContentType = "article",
            OriginalText = request.Body,
            IsPublished = true,
            CreatedAt = TimeHelper.GetVietnamTime()
        };

        var result = await _contentService.CreateContentAsync(content);
        return Ok(result);
    }

    [HttpPost("upload-music")]
    public async Task<IActionResult> UploadMusic([FromForm] MusicUploadRequest request)
    {
        if (!await IsAdmin()) return Forbid();

        var allowedExtensions = new[] { ".mp3", ".aac", ".wav", ".m4a", ".flac" };
        var extension = Path.GetExtension(request.AudioFile.FileName).ToLower();
        if (!allowedExtensions.Contains(extension)) return BadRequest("Unsupported audio format.");

        string audioUrl = await _storageService.UploadFileAsync(request.AudioFile.OpenReadStream(), request.AudioFile.FileName, "contents/audio");

        string? thumbnailUrl = null;
        if (request.Thumbnail != null)
        {
            thumbnailUrl = await _storageService.UploadFileAsync(request.Thumbnail.OpenReadStream(), request.Thumbnail.FileName, "contents/images");
        }

        string? lrcContent = null;
        if (request.LrcFile != null)
        {
            using var reader = new StreamReader(request.LrcFile.OpenReadStream());
            lrcContent = await reader.ReadToEndAsync();
        }

        var content = new Content
        {
            Id = Guid.NewGuid(),
            CategoryId = request.CategoryId,
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Description = request.Description,
            ThumbnailUrl = thumbnailUrl,
            AudioUrl = audioUrl,
            ContentType = "music",
            IsPublished = true,
            Metadata = new Dictionary<string, object> { { "artist", request.Artist ?? "" } },
            CreatedAt = TimeHelper.GetVietnamTime()
        };

        var result = await _contentService.CreateContentAsync(content, lrcContent);
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] bool isPublished)
    {
        if (!await IsAdmin()) return Forbid();
        await _contentService.UpdateStatusAsync(id, isPublished);
        return NoContent();
    }

    private string GenerateSlug(string phrase)
    {
        string str = phrase.ToLower();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim();
        str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-");
        return str + "-" + Guid.NewGuid().ToString().Substring(0, 8);
    }
}

public class ArticleUploadRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Body { get; set; } = string.Empty;
    public IFormFile? Thumbnail { get; set; }
    public IFormFile? Audio { get; set; }
}

public class MusicUploadRequest
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string? Description { get; set; }
    public IFormFile AudioFile { get; set; } = null!;
    public IFormFile? LrcFile { get; set; }
    public IFormFile? Thumbnail { get; set; }
}
