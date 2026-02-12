using Postgrest.Attributes;
using Postgrest.Models;

namespace Suzubun.Repository.Entities;

[Table("contents")]
public class Content : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("slug")]
    public string? Slug { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [Column("content_type")]
    public string ContentType { get; set; } = "article"; // article, music, script

    [Column("original_text")]
    public string? OriginalText { get; set; }

    [Column("audio_url")]
    public string? AudioUrl { get; set; }

    [Column("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [Column("is_published")]
    public bool IsPublished { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

[Table("content_lines")]
public class ContentLine : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("content_id")]
    public Guid ContentId { get; set; }

    [Column("start_time")]
    public float StartTime { get; set; }

    [Column("end_time")]
    public float? EndTime { get; set; }

    [Column("text_jp")]
    public string TextJp { get; set; } = string.Empty;

    [Column("text_vi")]
    public string? TextVi { get; set; }

    [Column("order_index")]
    public int OrderIndex { get; set; }
}
