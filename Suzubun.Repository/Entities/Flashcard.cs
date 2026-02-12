using Postgrest.Attributes;
using Postgrest.Models;

namespace Suzubun.Repository.Entities;

[Table("flashcard_decks")]
public class FlashcardDeck : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

[Table("flashcards")]
public class Flashcard : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("deck_id")]
    public Guid DeckId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("word")]
    public string Word { get; set; } = string.Empty;

    [Column("reading")]
    public string? Reading { get; set; }

    [Column("meaning")]
    public string? Meaning { get; set; }

    [Column("sino_vietnamese")]
    public string? SinoVietnamese { get; set; }

    [Column("example_sentence")]
    public string? ExampleSentence { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
