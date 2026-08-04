using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;


namespace LibraryApi.Services
{
    public class BookService
    {
        private readonly List<Book> _books = new()
        {
            new Book
            {
                Id = 1,
                Title = "Suç ve Ceza",
                Author = "Fyodor Dostoyevski",
                IsAvailable = true
            } ,
            new Book
            {
                Id = 2,
                Title = "1984",
                Author = "George Orwell",
                IsAvailable = true
            }
        };

        public List<Book> GetAll() { return _books; }
        public Book? GetById(int id)
        {
            foreach (Book book in _books)
            {
                if (book.Id == id) return book;        
            }
            return null;
        }

        public void AddBook(Book book)
        {
            book.Id = GetNextId();
            book.IsAvailable = true;
            _books.Add(book);
        }

        private int GetNextId()
        {
            int highestId = 0;

            foreach (Book book in _books)
            {
                if (book.Id > highestId)
                {
                    highestId = book.Id;
                }
            }

            return highestId + 1;
        }

        public bool Update(int id, Book updatedBook)
        {
            foreach (Book book in _books)
            {
                if (book.Id == id)
                {
                    book.Title = updatedBook.Title;
                    book.Author = updatedBook.Author;

                    return true;
                }
            }

            return false;
        }


        public bool Delete(int id)
        {
            Book? bookToDelete = null;

            foreach (Book book in _books)
            {
                if (book.Id == id)
                {
                    bookToDelete = book;
                    break;
                }
            }

            if (bookToDelete is null) return false;

            _books.Remove(bookToDelete);
            return true;
        }
    }
}
