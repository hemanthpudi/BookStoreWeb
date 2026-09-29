using BookStore.API.DTOs;
using BookStore.API.Services;
using BookStore.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] BookQueryParameters parameters)
        {
            var books = await _bookService.GetBooksAsync(parameters);

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook(CreateBookDto dto)
        {
            var book = await _bookService.CreateBookAsync(dto);

            return CreatedAtAction(
                nameof(GetBookById),
                new { id = book.Id },
                book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id,UpdateBookDto dto)
        {
            var book = await _bookService.UpdateBookAsync(id, dto);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var deleted = await _bookService.DeleteBookAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}