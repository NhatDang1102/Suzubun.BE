using Suzubun.Repository.Entities;
using Suzubun.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Suzubun.Service.Services;

public interface IDictionaryService
{
    Task<DictionaryResponse> LookUpAsync(string word, string context);
}

public class DictionaryService : IDictionaryService
{
    private readonly Supabase.Client _adminSupabase;
    private readonly IJapaneseService _japaneseService;

    public DictionaryService([FromKeyedServices("AdminClient")] Supabase.Client adminSupabase, IJapaneseService japaneseService)
    {
        _adminSupabase = adminSupabase;
        _japaneseService = japaneseService;
    }

    public async Task<DictionaryResponse> LookUpAsync(string word, string context)
    {
        // 1. Tìm trong Cache (Database) - Sử dụng AdminClient để bypass RLS
        var cached = await _adminSupabase.From<DictionaryCache>()
            .Where(x => x.BaseForm == word)
            .Single();

        if (cached != null)
        {
            return new DictionaryResponse
            {
                Translation = cached.Translation ?? "",
                SinoVietnamese = cached.SinoVietnamese ?? "",
                PartOfSpeech = cached.PartOfSpeech ?? ""
            };
        }

        // 2. Nếu không có, gọi OpenAI
        var result = await _japaneseService.GetWordDefinitionAsync(word, context);

        // 3. Lưu vào Cache để lần sau dùng
        var newCache = new DictionaryCache
        {
            Id = Guid.NewGuid(),
            BaseForm = word,
            Translation = result.Translation,
            SinoVietnamese = result.SinoVietnamese,
            PartOfSpeech = result.PartOfSpeech,
            CreatedAt = Suzubun.Service.Helpers.TimeHelper.GetVietnamTime()
        };
        await _adminSupabase.From<DictionaryCache>().Insert(newCache);

        return result;
    }
}
