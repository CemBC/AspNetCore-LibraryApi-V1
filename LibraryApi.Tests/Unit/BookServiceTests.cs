using LibraryApi.Data;
using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services;
using LibraryApi.Tests.Helpers;

namespace LibraryApi.Tests.Unit;

public class BookServiceTests
{
    [Fact]
    public async Task AddBook_CreatesAvailableBook()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        Book book =
            await service.AddBook(
                new CreateBookRequest
                {
                    Title = "Dune",
                    Author = "Frank Herbert"
                },
                UserId: 99);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbBook);

        Assert.Equal(
            book.Id,
            dbBook.Id);

        Assert.Equal(
            "Dune",
            dbBook.Title);

        Assert.Equal(
            "Frank Herbert",
            dbBook.Author);

        Assert.Equal(
            BookStatus.Available,
            dbBook.Status);
    }


    [Fact]
    public async Task GetById_WhenBookExists_ReturnsBook()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        Book book =
            TestServiceFactory.AddBook(context);

        Book result =
            await service.GetById(book.Id);

        Assert.Equal(
            book.Id,
            result.Id);

        Assert.Equal(
            "Dune",
            result.Title);
    }


    [Fact]
    public async Task GetById_WhenBookDoesNotExist_ThrowsNotFoundException()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetById(999));
    }


    [Fact]
    public async Task Update_UpdatesTitleAndAuthor()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        Book book =
            TestServiceFactory.AddBook(context);

        await service.Update(
            book.Id,
            new UpdateBookRequest
            {
                Title = "Dune Messiah",
                Author = "Frank Herbert"
            },
            UserId: 99);

        Book? dbBook =
            await context.Books.FindAsync(book.Id);

        Assert.NotNull(dbBook);

        Assert.Equal(
            "Dune Messiah",
            dbBook.Title);

        Assert.Equal(
            "Frank Herbert",
            dbBook.Author);
    }


    [Fact]
    public async Task Delete_RemovesBook()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        Book book =
            TestServiceFactory.AddBook(context);

        int bookId = book.Id;

        await service.Delete(
            bookId,
            UserId: 99);

        Book? dbBook =
            await context.Books.FindAsync(bookId);

        Assert.Null(dbBook);
    }


    [Fact]
    public async Task GetAllAsync_AppliesSearchStatusSortingAndPagination()
    {
        await using LibraryDbContext context =
            TestServiceFactory.CreateContext();

        BookService service =
            TestServiceFactory.CreateBookService(context);

        TestServiceFactory.AddBook(
            context,
            "Dune",
            "Frank Herbert",
            BookStatus.Available);

        TestServiceFactory.AddBook(
            context,
            "Dune Messiah",
            "Frank Herbert",
            BookStatus.Available);

        TestServiceFactory.AddBook(
            context,
            "1984",
            "George Orwell",
            BookStatus.Loaned);

        PagedResponse<BookResponse> result =
            await service.GetAllAsync(
                new BookQuery
                {
                    Page = 1,
                    PageSize = 1,
                    Search = "Dune",
                    Status = BookStatus.Available,
                    SortBy = "title",
                    Descending = true
                });

        Assert.Equal(
            2,
            result.TotalCount);

        Assert.Equal(
            2,
            result.TotalPages);

        Assert.Single(
            result.Items);

        Assert.Equal(
            "Dune Messiah",
            result.Items[0].Title);

        Assert.Equal(
            BookStatus.Available,
            result.Items[0].Status);
    }
}