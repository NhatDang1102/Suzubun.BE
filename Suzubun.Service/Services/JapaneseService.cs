using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Suzubun.Service.Models;

namespace Suzubun.Service.Services;

public interface IJapaneseService
{
    Task<string> TranslateSentenceAsync(string sentence);
    Task<DictionaryResponse> GetWordDefinitionAsync(string word, string contextSentence);
    Task<Stream> GenerateSpeechAsync(string text);
}

public class DictionaryResponse
{
    public string Translation { get; set; } = string.Empty;
    public string SinoVietnamese { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
}

public class JapaneseService : IJapaneseService
{
    private readonly string _openAiKey;
    private readonly HttpClient _httpClient;

    public JapaneseService(IOptions<AppOptions> options, IHttpClientFactory httpClientFactory)
    {
        _openAiKey = options.Value.OpenAI.ApiKey;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<Stream> GenerateSpeechAsync(string text)
    {
        var requestBody = new
        {
            model = "tts-1",
            input = text,
            voice = "alloy" // Giọng alloy rất phù hợp cho tin tức
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech");
        request.Headers.Add("Authorization", $"Bearer {_openAiKey}");
        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<string> TranslateSentenceAsync(string sentence)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "Bạn là một biên dịch viên tiếng Nhật sang tiếng Việt chuyên nghiệp." },
                new { role = "user", content = $"Hãy dịch câu sau sang tiếng Việt tự nhiên nhất: \"{sentence}\"" }
            }
        };

        return await CallOpenAiAsync(requestBody);
    }

    public async Task<DictionaryResponse> GetWordDefinitionAsync(string word, string contextSentence)
    {
        var prompt = $@"
        Dựa vào ngữ cảnh câu: ""{contextSentence}""
        Hãy cung cấp thông tin cho từ ""{word}"":
        1. Nghĩa tiếng Việt chính xác nhất.
        2. Âm Hán Việt (nếu có).
        3. Loại từ.
        Trả về kết quả dưới dạng JSON object với các key: translation, sino_vietnamese, part_of_speech.";

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = prompt } }
        };

        var jsonResult = await CallOpenAiAsync(requestBody);
        
        try 
        {
            return JsonSerializer.Deserialize<DictionaryResponse>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new DictionaryResponse();
        }
        catch
        {
            return new DictionaryResponse { Translation = jsonResult }; // Fallback nếu OpenAI ko trả về JSON chuẩn
        }
    }

    private async Task<string> CallOpenAiAsync(object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_openAiKey}");
        request.Content = JsonContent.Create(body);

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "";
    }
}
