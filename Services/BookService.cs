using LibraryApi.Data;
using LibraryApi.DTOs.Books;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services
{
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

        public async Task<Book?> GetById(int id)
        {
            return await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Book> AddBook(CreateBookRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author
            };

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();

            return book;
        }

        public async Task<bool> Update(int id, UpdateBookRequest request)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            book.Title = request.Title;
            book.Author = request.Author;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            Book? book = await _context.Books.FindAsync(id);

            if (book is null)
                return false;

            _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}