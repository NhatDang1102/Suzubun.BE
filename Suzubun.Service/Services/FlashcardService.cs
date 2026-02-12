using Suzubun.Repository.Entities;
using Suzubun.Service.Models;

namespace Suzubun.Service.Services;

public interface IFlashcardService
{
    Task<List<FlashcardDeck>> GetUserDecksAsync(Guid userId);
    Task<FlashcardDeck> CreateDeckAsync(FlashcardDeck deck);
    Task<List<Flashcard>> GetFlashcardsAsync(Guid deckId, Guid userId);
    Task<Flashcard> AddFlashcardAsync(Flashcard flashcard);
    Task DeleteFlashcardAsync(Guid id, Guid userId);
}

public class FlashcardService : IFlashcardService
{
    private readonly Supabase.Client _supabase;

    public FlashcardService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<FlashcardDeck>> GetUserDecksAsync(Guid userId)
    {
        var response = await _supabase.From<FlashcardDeck>().Where(x => x.UserId == userId).Get();
        return response.Models;
    }

    public async Task<FlashcardDeck> CreateDeckAsync(FlashcardDeck deck)
    {
        deck.Id = Guid.NewGuid();
        var response = await _supabase.From<FlashcardDeck>().Insert(deck);
        return response.Model!;
    }

    public async Task<List<Flashcard>> GetFlashcardsAsync(Guid deckId, Guid userId)
    {
        var response = await _supabase.From<Flashcard>()
            .Where(x => x.DeckId == deckId)
            .Where(x => x.UserId == userId)
            .Get();
        return response.Models;
    }

    public async Task<Flashcard> AddFlashcardAsync(Flashcard flashcard)
    {
        flashcard.Id = Guid.NewGuid();
        var response = await _supabase.From<Flashcard>().Insert(flashcard);
        return response.Model!;
    }

    public async Task DeleteFlashcardAsync(Guid id, Guid userId)
    {
        await _supabase.From<Flashcard>().Where(x => x.Id == id).Where(x => x.UserId == userId).Delete();
    }
}
