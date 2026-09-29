using BookStore.API.Models;

namespace BookStore.API.Repositories.IRepositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();

        Task<Category?> GetCategoryByIdAsync(int id);

        Task AddCategoryAsync(Category category);

        void UpdateCategory(Category category);

        void DeleteCategory(Category category);
        Task<bool> HasBooksAsync(int categoryId);
    }
}