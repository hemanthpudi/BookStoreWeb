using BookStore.API.Data;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BookStore.API.Repositories.RepositoryImpl
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BookStoreDbContext _context;

        public CategoryRepository(BookStoreDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
        }

        public void DeleteCategory(Category category)
        {
            _context.Categories.Remove(category);
        }

        public async Task<bool> HasBooksAsync(int categoryId)
        {
            return await _context.Books.AnyAsync<Book>(b => b.CategoryId == categoryId);
        }
    }
}