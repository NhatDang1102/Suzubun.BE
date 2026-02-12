using Suzubun.Repository.Entities;
using Suzubun.Service.Models;
using System.Text.RegularExpressions;

namespace Suzubun.Service.Services;

public interface IContentService
{
    Task<PagedResult<Content>> GetPagedContentsAsync(PaginationParams @params, string? type = null, Guid? categoryId = null, bool? onlyPublished = null);
    Task<Content?> GetDetailAsync(Guid id);
    Task<List<ContentLine>> GetLinesAsync(Guid contentId);
    Task<Content> CreateContentAsync(Content content, string? lrcContent = null);
    Task UpdateStatusAsync(Guid id, bool isPublished);
}

public class ContentService : IContentService
{
    private readonly Supabase.Client _supabase;

    public ContentService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<PagedResult<Content>> GetPagedContentsAsync(PaginationParams @params, string? type = null, Guid? categoryId = null, bool? onlyPublished = null)
    {
        var table = _supabase.From<Content>();
        
        if (!string.IsNullOrEmpty(type)) table.Where(x => x.ContentType == type);
        if (categoryId != null) table.Where(x => x.CategoryId == categoryId);
        if (onlyPublished.HasValue) table.Where(x => x.IsPublished == onlyPublished.Value);

        var countResponse = await table.Count(Postgrest.Constants.CountType.Exact);
        int totalCount = (int)countResponse;

        var from = (@params.PageNumber - 1) * @params.PageSize;
        var to = from + @params.PageSize - 1;
        
        var response = await table
            .Range(from, to)
            .Order("created_at", Postgrest.Constants.Ordering.Descending)
            .Get();

        return new PagedResult<Content>(response.Models, totalCount, @params.PageNumber, @params.PageSize);
    }

    public async Task<Content?> GetDetailAsync(Guid id)
    {
        var response = await _supabase.From<Content>().Where(x => x.Id == id).Single();
        return response;
    }

    public async Task<List<ContentLine>> GetLinesAsync(Guid contentId)
    {
        var response = await _supabase.From<ContentLine>()
            .Where(x => x.ContentId == contentId)
            .Order("order_index", Postgrest.Constants.Ordering.Ascending)
            .Get();
        return response.Models;
    }

    public async Task<Content> CreateContentAsync(Content content, string? lrcContent = null)
    {
        var response = await _supabase.From<Content>().Insert(content);
        var newContent = response.Model;

        if (!string.IsNullOrEmpty(lrcContent) && newContent != null)
        {
            var lines = ParseLrc(lrcContent, newContent.Id);
            await _supabase.From<ContentLine>().Insert(lines);
        }

        return newContent!;
    }

    public async Task UpdateStatusAsync(Guid id, bool isPublished)
    {
        await _supabase.From<Content>()
            .Where(x => x.Id == id)
            .Set(x => x.IsPublished, isPublished)
            .Update();
    }

    private List<ContentLine> ParseLrc(string lrcContent, Guid contentId)
    {
        var lines = new List<ContentLine>();
        var regex = new Regex(@"\[(?<time>\d{2}:\d{2}\.\d{2})\](?<text>.*)");
        var matches = regex.Matches(lrcContent);

        int index = 0;
        foreach (Match match in matches)
        {
            var timeStr = match.Groups["time"].Value;
            var text = match.Groups["text"].Value.Trim();
            var timeSpan = TimeSpan.ParseExact(timeStr, @"mm\:ss\.ff", null);
            
            lines.Add(new ContentLine
            {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                StartTime = (float)timeSpan.TotalSeconds,
                TextJp = text,
                OrderIndex = index++
            });
        }
        return lines;
    }
}
