using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc; 
using LibraryApi.Models;

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
        public async Task<ActionResult<List<Book>>> GetAll()
        {
            return await _bookService.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetById(int id)
        {
            var book = await _bookService.GetById(id);
            if (book == null) return NotFound();
            return book;
        }

        [HttpPost]
        public async Task<ActionResult<Book>> Create(Book book)
        {
            await _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, Book updatedBook)
        {
            bool isUpdated = await _bookService.Update(id, updatedBook);
            if (!isUpdated) return NotFound();
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
