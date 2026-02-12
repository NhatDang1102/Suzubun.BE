using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Suzubun.Repository.Entities;
using Suzubun.Service.Helpers;
using Suzubun.Service.Services;
using System.Security.Claims;

namespace Suzubun.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FlashcardController : ControllerBase
{
    private readonly IFlashcardService _flashcardService;

    public FlashcardController(IFlashcardService flashcardService)
    {
        _flashcardService = flashcardService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("decks")]
    public async Task<IActionResult> GetDecks()
    {
        return Ok(await _flashcardService.GetUserDecksAsync(GetUserId()));
    }

    [HttpPost("decks")]
    public async Task<IActionResult> CreateDeck([FromBody] FlashcardDeck deck)
    {
        deck.UserId = GetUserId();
        deck.CreatedAt = TimeHelper.GetVietnamTime();
        return Ok(await _flashcardService.CreateDeckAsync(deck));
    }

    [HttpGet("decks/{deckId}/cards")]
    public async Task<IActionResult> GetCards(Guid deckId)
    {
        return Ok(await _flashcardService.GetFlashcardsAsync(deckId, GetUserId()));
    }

    [HttpPost("cards")]
    public async Task<IActionResult> AddCard([FromBody] Flashcard card)
    {
        card.UserId = GetUserId();
        card.CreatedAt = TimeHelper.GetVietnamTime();
        return Ok(await _flashcardService.AddFlashcardAsync(card));
    }

    [HttpDelete("cards/{id}")]
    public async Task<IActionResult> DeleteCard(Guid id)
    {
        await _flashcardService.DeleteFlashcardAsync(id, GetUserId());
        return NoContent();
    }
}
