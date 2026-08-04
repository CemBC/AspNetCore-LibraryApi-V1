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
        public ActionResult<List<Book>> GetAll()
        {
            return _bookService.GetAll();
        }

        [HttpGet("{id}")]
        public ActionResult<Book> GetById(int id)
        {
            var book = _bookService.GetById(id);
            if (book == null) return NotFound();
            return book;
        }

        [HttpPost]
        public ActionResult<Book> Create(Book book)
        {
            _bookService.AddBook(book);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }


        [HttpPut("{id:int}")]
        public ActionResult Update(int id, Book updatedBook)
        {
            bool isUpdated = _bookService.Update(id, updatedBook);
            if (!isUpdated) return NotFound();
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            bool isDeleted = _bookService.Delete(id);

            if (!isDeleted) return NotFound();

            return NoContent();
        }
    }
}
