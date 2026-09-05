using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class BookService
{
    private readonly LibraryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<BookService> _logger;

    private readonly IBlobStorageService _blobStorageService;

    public BookService(LibraryDbContext context , IMapper mapper , ILogger<BookService> logger , IBlobStorageService blobStorageService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _blobStorageService = blobStorageService;
    }

    public async Task<string> UploadImageAsync(int bookId, IFormFile file, int userId)
    {
        Book? book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book is null)
        {
            throw new NotFoundException("Book not found.");
        }

        ValidateImage(file);

        string? oldImageUrl = book.ImageUrl;

        string newImageUrl = await _blobStorageService.UploadBookImageAsync(file);

        book.ImageUrl = newImageUrl;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _blobStorageService.DeleteAsync(newImageUrl);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldImageUrl))
        {
            await _blobStorageService.DeleteAsync(oldImageUrl);
        }

        _logger.LogInformation(
            "Image for book {BookId} uploaded by user {UserId}.",
            book.Id,
            userId);

        return newImageUrl;
    }

    public async Task DeleteImageAsync(int bookId, int userId)
    {
        Book? book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == bookId);

        if (book is null)
        {
            throw new NotFoundException("Book not found.");
        }

        if (string.IsNullOrWhiteSpace(book.ImageUrl))
        {
            throw new BadRequestException("Book does not have an image.");
        }

        string oldImageUrl = book.ImageUrl;

        book.ImageUrl = null;

        await _context.SaveChangesAsync();

        await _blobStorageService.DeleteAsync(oldImageUrl);

        _logger.LogInformation(
            "Image for book {BookId} deleted by user {UserId}.",
            book.Id,
            userId);
    }

    private static void ValidateImage(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new BadRequestException("Image file cannot be empty.");
        }

        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            throw new BadRequestException("Image cannot exceed 5 MB.");
        }

        string[] allowedContentTypes =
        {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

        if (!allowedContentTypes.Contains(file.ContentType))
        {
            throw new BadRequestException(
                "Only JPEG, PNG and WEBP images are allowed.");
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        string[] allowedExtensions =
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        if (!allowedExtensions.Contains(extension))
        {
            throw new BadRequestException(
                "Invalid image file extension.");
        }
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


    public async Task<Book> AddBook(CreateBookRequest request, int userId)
    {
        Book book = new Book
        {
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            Description = request.Description.Trim()
        };

        await _context.Books.AddAsync(book);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Book {BookId} created with title {BookTitle} by the User with ID:{UserId}",
            book.Id,
            book.Title,
            userId);

        return book;
    }


    public async Task Update(int id, UpdateBookRequest request, int userId)
    {
        Book? book = await _context.Books.FindAsync(id);

        if (book is null)
        {
            throw new NotFoundException("Book not found.");
        }

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.Description = request.Description.Trim();

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Book with ID: {BookId} updated with values {BookTitle}, {BookAuthor} by the User with ID: {UserId}",
            book.Id,
            book.Title,
            book.Author,
            userId);
    }


    public async Task Delete(int id, int UserId)
    {
        Book? book = await _context.Books.FindAsync(id);

        if (book is null)
            throw new NotFoundException("Book not found.");

        bool hasActiveLoan = await _context.Loans
            .AnyAsync(l =>
                l.BookId == id &&
                (l.Status == LoanStatus.Active ||
                 l.Status == LoanStatus.Overdue));

        if (hasActiveLoan)
        {
            throw new BadRequestException(
                "Book cannot be deleted while it has an active loan.");
        }

        _context.Books.Remove(book);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Book with ID: {BookId} deleted by the User with ID:{UserID}",
            book.Id,
            UserId);
    }
}