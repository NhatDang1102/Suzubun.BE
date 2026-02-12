namespace Suzubun.Service.Models;

public class SupabaseConfig
{
    public string Url { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string AnonKey { get; set; } = string.Empty;
}

public class CloudflareR2Options
{
    public string ServiceUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
}

public class OpenAiOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

public class SmtpOptions
{
    public string FromEmail { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string FromName { get; set; } = string.Empty;
}

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public class AppOptions
{
    public SupabaseConfig Supabase { get; set; } = new();
    public CloudflareR2Options CloudflareR2 { get; set; } = new();
    public OpenAiOptions OpenAI { get; set; } = new();
    public SmtpOptions Smtp { get; set; } = new();
    public JwtOptions Jwt { get; set; } = new();
}
