using BookStore.API.Data;
using BookStore.API.DTOs;
using BookStore.API.Models;
using BookStore.API.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BookStore.API.Repositories.RepositoryImpl
{
    public class BookRepository : IBookRepository
    {
        private readonly BookStoreDbContext _context;

        public BookRepository(BookStoreDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Book> Books, int TotalItems)>
     GetBooksAsync(BookQueryParameters parameters)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                query = query.Where(b =>
                    b.Title.Contains(parameters.Search) ||
                    b.Author.Contains(parameters.Search));
            }

            // Category filter
            if (parameters.CategoryId.HasValue)
            {
                query = query.Where(b =>
                    b.CategoryId == parameters.CategoryId.Value);
            }

            // Minimum price
            if (parameters.MinPrice.HasValue)
            {
                query = query.Where(b =>
                    b.Price >= parameters.MinPrice.Value);
            }

            // Maximum price
            if (parameters.MaxPrice.HasValue)
            {
                query = query.Where(b =>
                    b.Price <= parameters.MaxPrice.Value);
            }

            // Sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "title" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(b => b.Title)
                    : query.OrderBy(b => b.Title),

                "price" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(b => b.Price)
                    : query.OrderBy(b => b.Price),

                "author" => parameters.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(b => b.Author)
                    : query.OrderBy(b => b.Author),

                _ => query.OrderBy(b => b.Id)
            };

            // Total matching books before pagination
            var totalItems = await query.CountAsync();

            // Pagination
            var pageNumber = parameters.PageNumber < 1
                ? 1
                : parameters.PageNumber;

            var pageSize = parameters.PageSize < 1
                ? 10
                : Math.Min(parameters.PageSize, 50);

            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (books, totalItems);
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _context.Books.AddAsync(book);
        }

        public void UpdateBook(Book book)
        {
            _context.Books.Update(book);
        }

        public void DeleteBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        
    }
}