using Postgrest.Attributes;
using Postgrest.Models;

namespace Suzubun.Repository.Entities;

[Table("profiles")]
public class Profile : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("role")]
    public string Role { get; set; } = "user"; // admin, user

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
