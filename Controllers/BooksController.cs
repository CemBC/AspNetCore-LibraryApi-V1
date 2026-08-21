using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc; 
using LibraryApi.Models;
using LibraryApi.DTOs.Books;

namespace LibraryApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        public BooksController(BookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookResponse>>> GetAll()
        {
            var books = await _bookService.GetAll();
            var response = books.Select(book => new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                IsAvailable = book.IsAvailable
            }).ToList();

            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookResponse>> GetById(int id)
        {
            var book = await _bookService.GetById(id);

            if (book == null)
                return NotFound();

            var response = new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                IsAvailable = book.IsAvailable
            };

            return response;
        }

        [HttpPost]
        public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request)
        {
            var book = await _bookService.AddBook(request);

            var response = new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                IsAvailable = book.IsAvailable
            };

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, response);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateBookRequest request)
        {
            var updated = await _bookService.Update(id, request);

            if (!updated)
                return NotFound();

            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            bool isDeleted = await _bookService.Delete(id);

            if (!isDeleted) return NotFound();

            return NoContent();
        }
    }
}
