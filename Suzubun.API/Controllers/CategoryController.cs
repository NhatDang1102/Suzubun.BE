using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Suzubun.Repository.Entities;
using Suzubun.Service.Services;
using System.Security.Claims;

namespace Suzubun.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly Supabase.Client _adminSupabase;

    public CategoryController(ICategoryService categoryService, [FromKeyedServices("AdminClient")] Supabase.Client adminSupabase)
    {
        _categoryService = categoryService;
        _adminSupabase = adminSupabase;
    }

    private async Task<bool> IsAdmin()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return false;

        var profile = await _adminSupabase.From<Profile>().Where(x => x.Id == Guid.Parse(userId)).Single();
        return profile?.Role == "admin";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _categoryService.GetAllAsync());

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Category category) 
    {
        if (!await IsAdmin()) return Forbid();
        category.CreatedAt = Suzubun.Service.Helpers.TimeHelper.GetVietnamTime();
        return Ok(await _categoryService.CreateAsync(category));
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await IsAdmin()) return Forbid();
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
