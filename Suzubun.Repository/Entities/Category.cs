using Postgrest.Attributes;
using Postgrest.Models;

namespace Suzubun.Repository.Entities;

[Table("categories")]
public class Category : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("type")]
    public string Type { get; set; } = "article"; // article, music, script

    [Column("icon")]
    public string? Icon { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
