using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService
{
    private readonly LibraryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;
    public BookService(LibraryDbContext context , IMapper mapper , ILogger<BookService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PagedResponse<BookResponse>> GetAllAsync(BookQuery query)
    {
        IQueryable<Book> booksQuery = _context.Books.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            booksQuery = booksQuery.Where(b => b.Title.Contains(search) ||b.Author.Contains(search));
        }

        if (query.Status.HasValue) booksQuery = booksQuery.Where(b => b.Status == query.Status.Value);

        booksQuery = query.SortBy?.ToLower() switch
        {
            "title" => query.Descending ? booksQuery.OrderByDescending(b => b.Title) : booksQuery.OrderBy(b => b.Title),

            "author" => query.Descending ? booksQuery.OrderByDescending(b => b.Author) : booksQuery.OrderBy(b => b.Author),

            _ => booksQuery.OrderBy(b => b.Id)
        };

        int totalCount = await booksQuery.CountAsync();

        List<Book> books =  await booksQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<BookResponse>
        {
            Items = _mapper.Map<List<BookResponse>>(books),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages =(int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }


    public async Task<Book> GetById(int id)
    {
        Book? book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);


        if (book is null)
            throw new NotFoundException("Book not found.");


        return book;
    }


    public async Task<Book> AddBook(CreateBookRequest request , int UserId)
    {
        Book book = new Book
        {
            Title = request.Title,
            Author = request.Author
        };


        await _context.Books.AddAsync(book);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Book {BookId} created with title {BookTitle} by the User with ID:{UserId}",
            book.Id,
            book.Title,
            UserId);

        return book;
    }


    public async Task Update(int id, UpdateBookRequest request , int UserId)
    {
        Book? book = await _context.Books.FindAsync(id);


        if (book is null)
            throw new NotFoundException("Book not found.");


        book.Title = request.Title;
        book.Author = request.Author;
        
        

        await _context.SaveChangesAsync();

        _logger.LogInformation("Book with ID:  {BookId} updated with values {BookTitle} , {BookAuthor} by the User with ID: {UserId}", book.Id, book.Title, book.Author, UserId);
    }


    public async Task Delete(int id , int UserId)
    {
        Book? book = await _context.Books.FindAsync(id);


        if (book is null)
            throw new NotFoundException("Book not found.");


        _context.Books.Remove(book);

        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Book with ID: {BookId} deleted by the User with ID:{UserID}",
            book.Id,
            UserId);

    }
}