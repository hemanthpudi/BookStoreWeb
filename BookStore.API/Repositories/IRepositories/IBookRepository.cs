using BookStore.API.DTOs;
using BookStore.API.Models;

namespace BookStore.API.Repositories.IRepositories
{
    public interface IBookRepository
    {
        Task<(IEnumerable<Book>Books,int TotalItems)> GetBooksAsync(BookQueryParameters parameters);

        Task<Book?> GetBookByIdAsync(int id);

        Task AddBookAsync(Book book);

        void UpdateBook(Book book);

        void DeleteBook(Book book);

        Task SaveChangesAsync();
    }
}