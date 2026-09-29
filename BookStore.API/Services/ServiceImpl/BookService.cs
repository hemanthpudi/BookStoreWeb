using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories;
using BookStore.API.Repositories.IRepositories;
using BookStore.API.Services.IServices;

namespace BookStore.API.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Get all books
        public async Task<PagedResult<BookDto>> GetBooksAsync(BookQueryParameters parameters)
        {
            var result =await _unitOfWork.Books.GetBooksAsync(parameters);

            var books = result.Books;

            var totalItems = result.TotalItems;

            var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;

            var pageSize = parameters.PageSize < 1? 10: Math.Min(parameters.PageSize, 50);

            var bookDtos = books.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Description = b.Description,
                ISBN = b.ISBN,
                Price = b.Price,
                StockQuantity = b.StockQuantity,
                ImageUrl = b.ImageUrl,
                CategoryId = b.CategoryId,
                CategoryName = b.Category.Name
            });

            return new PagedResult<BookDto>
            {
                Items = bookDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(
                    totalItems / (double)pageSize)
            };
        }

        // GET: Get book by ID
        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.GetBookByIdAsync(id);

            if (book == null)
            {
                return null;
            }

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                ISBN = book.ISBN,
                Price = book.Price,
                StockQuantity = book.StockQuantity,
                ImageUrl = book.ImageUrl,
                CategoryId = book.CategoryId,
                CategoryName = book.Category.Name
            };
        }

        // POST: Create book
        public async Task<BookDto> CreateBookAsync(CreateBookDto dto)
        {
            var category = await _unitOfWork.Categories.GetCategoryByIdAsync(dto.CategoryId);
            if (category==null)
            {
                throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found.");
            }
            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                ISBN = dto.ISBN,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId
            };

            await _unitOfWork.Books.AddBookAsync(book);

            await _unitOfWork.SaveChangesAsync();

            return await GetBookByIdAsync(book.Id)
                ?? throw new Exception(
                    "Book could not be retrieved after creation.");
        }

        // PUT: Update book
        public async Task<BookDto?> UpdateBookAsync(
            int id,
            UpdateBookDto dto)
        {
            var book = await _unitOfWork.Books.GetBookByIdAsync(id);

            if (book == null)
            {
                return null;
            }
            var category = await _unitOfWork.Categories.GetCategoryByIdAsync(dto.CategoryId);
            if(category==null)
            {
                throw new KeyNotFoundException($"Category with id {dto.CategoryId} not found.");
            }

            book.Title = dto.Title;
            book.Author = dto.Author;
            book.Description = dto.Description;
            book.ISBN = dto.ISBN;
            book.Price = dto.Price;
            book.StockQuantity = dto.StockQuantity;
            book.ImageUrl = dto.ImageUrl;
            book.CategoryId = dto.CategoryId;

            _unitOfWork.Books.UpdateBook(book);

            await _unitOfWork.SaveChangesAsync();

            return await GetBookByIdAsync(book.Id);
        }

        // DELETE: Delete book
        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _unitOfWork.Books.GetBookByIdAsync(id);

            if (book == null)
            {
                return false;
            }

            _unitOfWork.Books.DeleteBook(book);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}