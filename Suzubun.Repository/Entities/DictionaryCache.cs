using Postgrest.Attributes;
using Postgrest.Models;

namespace Suzubun.Repository.Entities;

[Table("dictionary_cache")]
public class DictionaryCache : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("base_form")]
    public string BaseForm { get; set; } = string.Empty;

    [Column("reading")]
    public string? Reading { get; set; }

    [Column("translation")]
    public string? Translation { get; set; }

    [Column("sino_vietnamese")]
    public string? SinoVietnamese { get; set; }

    [Column("part_of_speech")]
    public string? PartOfSpeech { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
