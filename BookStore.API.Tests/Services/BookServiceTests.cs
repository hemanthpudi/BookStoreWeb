using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookStore.API.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _bookRepositoryMock = new Mock<IBookRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            _unitOfWorkMock
                .SetupGet(x => x.Books)
                .Returns(_bookRepositoryMock.Object);

            _unitOfWorkMock
                .SetupGet(x => x.Categories)
                .Returns(_categoryRepositoryMock.Object);

            _bookService = new BookService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetBooksAsync_WhenParametersAreValid_ShouldReturnPagedMappedBooks()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            var books = new List<Book>
            {
                new Book
                {
                    Id = 1,
                    Title = "Book 1",
                    Author = "Author 1",
                    Description = "Desc 1",
                    ISBN = "ISBN-1",
                    Price = 100,
                    StockQuantity = 5,
                    ImageUrl = "image-1",
                    CategoryId = 1,
                    Category = category
                },
                new Book
                {
                    Id = 2,
                    Title = "Book 2",
                    Author = "Author 2",
                    Description = "Desc 2",
                    ISBN = "ISBN-2",
                    Price = 200,
                    StockQuantity = 3,
                    ImageUrl = "image-2",
                    CategoryId = 1,
                    Category = category
                }
            };

            var parameters = new BookQueryParameters
            {
                Search = "Book",
                CategoryId = 1,
                MinPrice = 50,
                MaxPrice = 250,
                SortBy = "title",
                SortOrder = "asc",
                PageNumber = 2,
                PageSize = 5
            };

            _bookRepositoryMock
                .Setup(x => x.GetBooksAsync(parameters))
                .ReturnsAsync((books, 12));

            // Act
            var result = await _bookService.GetBooksAsync(parameters);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalItems.Should().Be(12);
            result.TotalPages.Should().Be(3);
            result.Items.Should().BeEquivalentTo(new[]
            {
                new BookDto
                {
                    Id = 1,
                    Title = "Book 1",
                    Author = "Author 1",
                    Description = "Desc 1",
                    ISBN = "ISBN-1",
                    Price = 100,
                    StockQuantity = 5,
                    ImageUrl = "image-1",
                    CategoryId = 1,
                    CategoryName = "Fiction"
                },
                new BookDto
                {
                    Id = 2,
                    Title = "Book 2",
                    Author = "Author 2",
                    Description = "Desc 2",
                    ISBN = "ISBN-2",
                    Price = 200,
                    StockQuantity = 3,
                    ImageUrl = "image-2",
                    CategoryId = 1,
                    CategoryName = "Fiction"
                }
            });
        }

        [Fact]
        public async Task GetBooksAsync_WhenPagingValuesAreBelowMinimum_ShouldNormalizePaging()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            var parameters = new BookQueryParameters
            {
                PageNumber = 0,
                PageSize = 0
            };

            _bookRepositoryMock
                .Setup(x => x.GetBooksAsync(parameters))
                .ReturnsAsync((new List<Book>
                {
                    new Book
                    {
                        Id = 1,
                        Title = "Book 1",
                        Author = "Author 1",
                        Price = 100,
                        StockQuantity = 2,
                        CategoryId = 1,
                        Category = category
                    }
                }, 1));

            // Act
            var result = await _bookService.GetBooksAsync(parameters);

            // Assert
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalPages.Should().Be(1);
        }

        [Fact]
        public async Task GetBooksAsync_WhenPagingValuesExceedMaximum_ShouldClampPageSizeToFifty()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            var parameters = new BookQueryParameters
            {
                PageNumber = 1,
                PageSize = 100
            };

            _bookRepositoryMock
                .Setup(x => x.GetBooksAsync(parameters))
                .ReturnsAsync((new List<Book>
                {
                    new Book
                    {
                        Id = 1,
                        Title = "Book 1",
                        Author = "Author 1",
                        Price = 100,
                        StockQuantity = 2,
                        CategoryId = 1,
                        Category = category
                    }
                }, 51));

            // Act
            var result = await _bookService.GetBooksAsync(parameters);

            // Assert
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(50);
            result.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnMappedDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            var book = new Book
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Description = "Desc 1",
                ISBN = "ISBN-1",
                Price = 100,
                StockQuantity = 5,
                ImageUrl = "image-1",
                CategoryId = 1,
                Category = category
            };

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            // Act
            var result = await _bookService.GetBookByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(new BookDto
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Description = "Desc 1",
                ISBN = "ISBN-1",
                Price = 100,
                StockQuantity = 5,
                ImageUrl = "image-1",
                CategoryId = 1,
                CategoryName = "Fiction"
            });
        }

        [Fact]
        public async Task GetBookByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(99))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.GetBookByIdAsync(99);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateBookAsync_WhenCategoryExists_ShouldCreateBookAndReturnDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Fiction"
            };

            var dto = new CreateBookDto
            {
                Title = "Book 1",
                Author = "Author 1",
                Description = "Desc 1",
                ISBN = "ISBN-1",
                Price = 100,
                StockQuantity = 5,
                ImageUrl = "image-1",
                CategoryId = 1
            };

            var createdBook = new Book
            {
                Id = 1,
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                ISBN = dto.ISBN,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                Category = category
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(1))
                .ReturnsAsync(category);

            _bookRepositoryMock
                .Setup(x => x.AddBookAsync(It.IsAny<Book>()))
                .Callback<Book>(book => book.Id = 1)
                .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(createdBook);

            // Act
            var result = await _bookService.CreateBookAsync(dto);

            // Assert
            result.Should().BeEquivalentTo(new BookDto
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Description = "Desc 1",
                ISBN = "ISBN-1",
                Price = 100,
                StockQuantity = 5,
                ImageUrl = "image-1",
                CategoryId = 1,
                CategoryName = "Fiction"
            });

            _bookRepositoryMock.Verify(
                x => x.AddBookAsync(It.Is<Book>(book =>
                    book.Title == dto.Title &&
                    book.Author == dto.Author &&
                    book.CategoryId == dto.CategoryId)),
                Times.Once);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }

        [Fact]
        public async Task CreateBookAsync_WhenCategoryDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var dto = new CreateBookDto
            {
                Title = "Book 1",
                Author = "Author 1",
                Price = 100,
                StockQuantity = 5,
                CategoryId = 99
            };

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(99))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _bookService.CreateBookAsync(dto);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Category with id 99 not found.");

            _bookRepositoryMock.Verify(
                x => x.AddBookAsync(It.IsAny<Book>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBookAsync_WhenBookExistsAndCategoryExists_ShouldUpdateAndReturnDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 2,
                Name = "Technology"
            };

            var book = new Book
            {
                Id = 1,
                Title = "Old Title",
                Author = "Old Author",
                Description = "Old Desc",
                ISBN = "OLD",
                Price = 50,
                StockQuantity = 1,
                ImageUrl = "old-image",
                CategoryId = 1,
                Category = category
            };

            var dto = new UpdateBookDto
            {
                Title = "New Title",
                Author = "New Author",
                Description = "New Desc",
                ISBN = "NEW",
                Price = 150,
                StockQuantity = 7,
                ImageUrl = "new-image",
                CategoryId = 2
            };

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(2))
                .ReturnsAsync(category);

            _bookRepositoryMock
                .Setup(x => x.UpdateBook(book));

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            // Act
            var result = await _bookService.UpdateBookAsync(1, dto);

            // Assert
            result.Should().BeEquivalentTo(new BookDto
            {
                Id = 1,
                Title = "New Title",
                Author = "New Author",
                Description = "New Desc",
                ISBN = "NEW",
                Price = 150,
                StockQuantity = 7,
                ImageUrl = "new-image",
                CategoryId = 2,
                CategoryName = "Technology"
            });

            book.Title.Should().Be("New Title");
            book.Author.Should().Be("New Author");
            book.CategoryId.Should().Be(2);
            _bookRepositoryMock.Verify(x => x.UpdateBook(book), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_WhenBookDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var dto = new UpdateBookDto
            {
                Title = "New Title",
                Author = "New Author",
                Price = 150,
                StockQuantity = 7,
                CategoryId = 2
            };

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.UpdateBookAsync(1, dto);

            // Assert
            result.Should().BeNull();
            _categoryRepositoryMock.Verify(
                x => x.GetCategoryByIdAsync(It.IsAny<int>()),
                Times.Never);
            _bookRepositoryMock.Verify(
                x => x.UpdateBook(It.IsAny<Book>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateBookAsync_WhenCategoryDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Title = "Old Title",
                Author = "Old Author",
                Price = 50,
                StockQuantity = 1,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Fiction" }
            };

            var dto = new UpdateBookDto
            {
                Title = "New Title",
                Author = "New Author",
                Price = 150,
                StockQuantity = 7,
                CategoryId = 99
            };

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            _categoryRepositoryMock
                .Setup(x => x.GetCategoryByIdAsync(99))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _bookService.UpdateBookAsync(1, dto);

            // Assert
            await act.Should()
                .ThrowAsync<KeyNotFoundException>()
                .WithMessage("Category with id 99 not found.");

            _bookRepositoryMock.Verify(
                x => x.UpdateBook(It.IsAny<Book>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteBookAsync_WhenBookDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.DeleteBookAsync(1);

            // Assert
            result.Should().BeFalse();
            _bookRepositoryMock.Verify(
                x => x.DeleteBook(It.IsAny<Book>()),
                Times.Never);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Never);
        }

        [Fact]
        public async Task DeleteBookAsync_WhenBookExists_ShouldDeleteAndReturnTrue()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Title = "Book 1",
                Author = "Author 1",
                Price = 100,
                StockQuantity = 2,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Fiction" }
            };

            _bookRepositoryMock
                .Setup(x => x.GetBookByIdAsync(1))
                .ReturnsAsync(book);

            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _bookService.DeleteBookAsync(1);

            // Assert
            result.Should().BeTrue();
            _bookRepositoryMock.Verify(
                x => x.DeleteBook(book),
                Times.Once);
            _unitOfWorkMock.Verify(
                x => x.SaveChangesAsync(),
                Times.Once);
        }
    }
}
