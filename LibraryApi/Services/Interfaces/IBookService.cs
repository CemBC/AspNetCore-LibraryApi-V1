using LibraryApi.DTOs.Books;
using LibraryApi.DTOs.Common;
using LibraryApi.Models;
using Microsoft.AspNetCore.Http;

namespace LibraryApi.Services.Interfaces;

public interface IBookService
{
    Task<string> UploadImageAsync(int bookId,IFormFile file,int userId);

    Task DeleteImageAsync(int bookId,int userId);

    Task<PagedResponse<BookResponse>> GetAllAsync(BookQuery query);

    Task<Book> GetById(int id);

    Task<Book> AddBook(CreateBookRequest request,int userId);

    Task Update(int id, UpdateBookRequest request,int userId);

    Task Delete(int id, int userId);
}