using BookStore.API.DTOs;

namespace BookStore.API.Services.IServices
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();

        Task<CategoryDto?> GetCategoryByIdAsync(int id);

        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto);

        Task<CategoryDto?> UpdateCategoryAsync(int id,UpdateCategoryDto dto);

        Task<bool> DeleteCategoryAsync(int id);
    }
}