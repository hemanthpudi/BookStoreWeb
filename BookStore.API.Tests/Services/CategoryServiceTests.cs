using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.ServiceImpl;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.API.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly CategoryService _categoryService;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            _unitOfWorkMock
                .SetupGet(x => x.Categories)
                .Returns(_categoryRepositoryMock.Object);

            _categoryService = new CategoryService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetCategoriesAsync_WhenCategoriesExist_ShouldReturnMappedDtos()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Fiction",
                    Description = "Story books"
                },
                new Category
                {
                    Id = 2,
                    Name = "Technology",
                    Description = "Technical books"
                }
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _categoryService.GetCategoriesAsync();

            // Assert
            result.Should().BeEquivalentTo(new[]
            {
                new CategoryDto
                {
                    Id = 1,
                    Name = "Fiction",
                    Description = "Story books"
                },
                new CategoryDto
                {
                    Id = 2,
                    Name = "Technology",
                    Description = "Technical books"
                }
            });
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenCategoryExists_ShouldReturnMappedDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction",
                Description = "Story books"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new CategoryDto
            {
                Id = 1,
                Name = "Fiction",
                Description = "Story books"
            });
        }

        [Fact]
        public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(99))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.GetCategoryByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateCategoryAsync_WhenValidDto_ShouldAddCategoryAndReturnDto()
        {
            // Arrange
            var dto = new CreateCategoryDto
            {
                Name = "Science",
                Description = "Science books"
            };

            Category? addedCategory = null;

            _categoryRepositoryMock
                .Setup(x => x.AddCategoryAsync(It.IsAny<Category>()))
                .Callback<Category>(category =>
                {
                    category.Id = 10;
                    addedCategory = category;
                })
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.CreateCategoryAsync(dto);

            // Assert
            result.Should().BeEquivalentTo(new CategoryDto
            {
                Id = 10,
                Name = "Science",
                Description = "Science books"
            });

            addedCategory.Should().NotBeNull();
            addedCategory!.Name.Should().Be("Science");
            addedCategory.Description.Should().Be("Science books");

            _categoryRepositoryMock.Verify(
                x => x.AddCategoryAsync(It.Is<Category>(category =>
                    category.Name == "Science" &&
                    category.Description == "Science books")),
                Times.Once);

            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenCategoryExists_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction",
                Description = "Old description"
            };

            var dto = new UpdateCategoryDto
            {
                Name = "Updated Fiction",
                Description = "New description"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(1, dto);

            // Assert
            result.Should().BeEquivalentTo(new CategoryDto
            {
                Id = 1,
                Name = "Updated Fiction",
                Description = "New description"
            });

            category.Name.Should().Be("Updated Fiction");
            category.Description.Should().Be("New description");

            _categoryRepositoryMock.Verify(
                x => x.UpdateCategory(category),
                Times.Once);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategoryAsync_WhenCategoryDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var dto = new UpdateCategoryDto
            {
                Name = "Updated Fiction",
                Description = "New description"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.UpdateCategoryAsync(1, dto);

            // Assert
            result.Should().BeNull();
            _categoryRepositoryMock.Verify(
                x => x.UpdateCategory(It.IsAny<Category>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync((Category?)null);

            // Act
            var result = await _categoryService.DeleteCategoryAsync(1);

            // Assert
            result.Should().BeFalse();
            _categoryRepositoryMock.Verify(
                x => x.HasBooksAsync(It.IsAny<int>()),
                Times.Never);
            _categoryRepositoryMock.Verify(
                x => x.DeleteCategory(It.IsAny<Category>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryHasBooks_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.HasBooksAsync(1))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _categoryService.DeleteCategoryAsync(1);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot delete category with id 1 because it has associated books.");

            _categoryRepositoryMock.Verify(
                x => x.DeleteCategory(It.IsAny<Category>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteCategoryAsync_WhenCategoryHasNoBooks_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            _categoryRepositoryMock
                .Setup(x => x.HasBooksAsync(1))
                .ReturnsAsync(false);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _categoryService.DeleteCategoryAsync(1);

            // Assert
            result.Should().BeTrue();
            _categoryRepositoryMock.Verify(
                x => x.DeleteCategory(category),
                Times.Once);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
