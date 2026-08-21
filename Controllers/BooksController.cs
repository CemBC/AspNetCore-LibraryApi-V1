using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc; 
using LibraryApi.Models;
using LibraryApi.DTOs.Books;
using FluentValidation;
using AutoMapper;
namespace LibraryApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IValidator<CreateBookRequest> _createValidator;
        private readonly IValidator<UpdateBookRequest> _updateValidator;
        private readonly IMapper _mapper;
        private readonly BookService _bookService;

        public BooksController(BookService bookService, IValidator<CreateBookRequest> createValidator , IValidator<UpdateBookRequest> updateValidator , IMapper mapper)
        {
            _bookService = bookService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookResponse>>> GetAll()
        {
            var books = await _bookService.GetAll();
            var response = _mapper.Map<List<BookResponse>>(books);

            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookResponse>> GetById(int id)
        {
            var book = await _bookService.GetById(id);

            if (book == null)
                return NotFound();

            var response = _mapper.Map<BookResponse>(book);

            return response;
        }

        [HttpPost]
        public async Task<ActionResult<BookResponse>> Create(CreateBookRequest request)
        {

            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var book = await _bookService.AddBook(request);

            var response = _mapper.Map<BookResponse>(book);

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, response);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateBookRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

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
