using Suzubun.Repository.Entities;
using Suzubun.Service.Models;

namespace Suzubun.Service.Services;

public interface ICategoryService
{
    Task<List<Category>> GetAllAsync();
    Task<Category> CreateAsync(Category category);
    Task DeleteAsync(Guid id);
}

public class CategoryService : ICategoryService
{
    private readonly Supabase.Client _supabase;

    public CategoryService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var response = await _supabase.From<Category>().Get();
        return response.Models;
    }

    public async Task<Category> CreateAsync(Category category)
    {
        category.Id = Guid.NewGuid();
        var response = await _supabase.From<Category>().Insert(category);
        return response.Model!;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _supabase.From<Category>().Where(x => x.Id == id).Delete();
    }
}
