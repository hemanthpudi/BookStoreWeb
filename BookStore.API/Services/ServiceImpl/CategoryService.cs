using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.IServices;

namespace BookStore.API.Services.ServiceImpl
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories =
                await _unitOfWork.Categories.GetCategoriesAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category =
                await _unitOfWork.Categories.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return null;
            }

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(
            CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            await _unitOfWork.Categories.AddCategoryAsync(category);

            await _unitOfWork.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(
            int id,
            UpdateCategoryDto dto)
        {
            var category =
                await _unitOfWork.Categories.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return null;
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            _unitOfWork.Categories.UpdateCategory(category);

            await _unitOfWork.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category =
                await _unitOfWork.Categories.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return false;
            }
            var hasBooks = await _unitOfWork.Categories.HasBooksAsync(id);
            if(hasBooks)
            {
                throw new InvalidOperationException($"Cannot delete category with id {id} because it has associated books.");
            }

            _unitOfWork.Categories.DeleteCategory(category);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}