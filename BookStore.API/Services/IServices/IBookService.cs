using BookStore.API.DTOs;

namespace BookStore.API.Services.IServices
{
    public interface IBookService
    {
        Task<PagedResult<BookDto>> GetBooksAsync(BookQueryParameters parameters);
        Task<BookDto?> GetBookByIdAsync(int id);
        Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
        Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto updateBookDto);
        Task<bool> DeleteBookAsync(int id);
    }
}
