using LibraryApi.Data;
using LibraryApi.DTOs.Books;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }


    public async Task<List<Book>> GetAll()
    {
        return await _context.Books
            .AsNoTracking()
            .ToListAsync();
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


    public async Task<Book> AddBook(CreateBookRequest request)
    {
        Book book = new Book
        {
            Title = request.Title,
            Author = request.Author
        };


        await _context.Books.AddAsync(book);

        await _context.SaveChangesAsync();


        return book;
    }


    public async Task Update(int id, UpdateBookRequest request)
    {
        Book? book = await _context.Books.FindAsync(id);


        if (book is null)
            throw new NotFoundException("Book not found.");


        book.Title = request.Title;
        book.Author = request.Author;


        await _context.SaveChangesAsync();
    }


    public async Task Delete(int id)
    {
        Book? book = await _context.Books.FindAsync(id);


        if (book is null)
            throw new NotFoundException("Book not found.");


        _context.Books.Remove(book);


        await _context.SaveChangesAsync();
    }
}