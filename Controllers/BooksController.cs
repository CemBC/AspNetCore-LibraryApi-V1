using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Books;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;
    private readonly IValidator<CreateBookRequest> _createValidator;
    private readonly IValidator<UpdateBookRequest> _updateValidator;
    private readonly IMapper _mapper;


    public BooksController(
        BookService bookService,
        IValidator<CreateBookRequest> createValidator,
        IValidator<UpdateBookRequest> updateValidator,
        IMapper mapper)
    {
        _bookService = bookService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<List<BookResponse>>> GetAll()
    {
        List<Book> books = await _bookService.GetAll();

        List<BookResponse> response =
            _mapper.Map<List<BookResponse>>(books);


        return Ok(response);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id)
    {
        Book book = await _bookService.GetById(id);


        BookResponse response =
            _mapper.Map<BookResponse>(book);


        return Ok(response);
    }



    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(
        CreateBookRequest request)
    {
        var validationResult =
            await _createValidator.ValidateAsync(request);


        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);


        Book book = await _bookService.AddBook(request);


        BookResponse response =
            _mapper.Map<BookResponse>(book);


        return CreatedAtAction(
            nameof(GetById),
            new { id = book.Id },
            response);
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UpdateBookRequest request)
    {
        var validationResult =
            await _updateValidator.ValidateAsync(request);


        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);


        await _bookService.Update(id, request);


        return NoContent();
    }



    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookService.Delete(id);

        return NoContent();
    }
}