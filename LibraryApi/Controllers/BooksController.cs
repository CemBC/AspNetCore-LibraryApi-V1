using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private readonly BookService _bookService;
    private readonly IValidator<CreateBookRequest> _createValidator;
    private readonly IValidator<UpdateBookRequest> _updateValidator;

    private readonly IValidator<BookQuery> _bookQueryValidator;
    private readonly IMapper _mapper;


    public BooksController(
        BookService bookService,
        IValidator<CreateBookRequest> createValidator,
        IValidator<UpdateBookRequest> updateValidator,
        IMapper mapper,
        IValidator<BookQuery> bookQueryValidator)
    {
        _bookService = bookService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _mapper = mapper;
        _bookQueryValidator = bookQueryValidator;
    }





    [HttpGet]
    public async Task<ActionResult<PagedResponse<BookResponse>>> GetAll([FromQuery] BookQuery query)
    {
        var validationResult = await _bookQueryValidator.ValidateAsync(query);

        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var response = await _bookService.GetAllAsync(query);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookResponse>> GetById(int id)
    {
        Book book = await _bookService.GetById(id);

        BookResponse response = _mapper.Map<BookResponse>(book);

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


        Book book = await _bookService.AddBook(request , CurrentUserId);


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


        await _bookService.Update(id, request , CurrentUserId);


        return NoContent();
    }



    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookService.Delete(id , CurrentUserId);

        return NoContent();
    }
}