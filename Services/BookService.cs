using LibraryApi.Models;
using Microsoft.Data.SqlClient;

namespace LibraryApi.Services
{
    public class BookService
    {
        private readonly string _connectionString;

        public BookService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("LibraryDb")!;
        }

        public List<Book> GetAll()
        {
            List<Book> books = new();

            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Title, Author, IsAvailable FROM Books";

            using SqlCommand command = new SqlCommand(query, connection);

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Book book = new Book
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    IsAvailable = reader.GetBoolean(3)
                };

                books.Add(book);
            }

            return books;
        }

        public Book? GetById(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = "SELECT Id, Title, Author, IsAvailable FROM Books WHERE Id = @Id";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new Book
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Author = reader.GetString(2),
                    IsAvailable = reader.GetBoolean(3)
                };
            }

            return null;
        }

        public void AddBook(Book book)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string query =
                @"INSERT INTO Books (Title, Author, IsAvailable)
                OUTPUT INSERTED.Id
                VALUES (@Title, @Author, 1)";

            using SqlCommand command =new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Title", book.Title);
            command.Parameters.AddWithValue("@Author", book.Author);

            book.Id = Convert.ToInt32(command.ExecuteScalar());
            book.IsAvailable = true;
        }

        public bool Update(int id, Book updatedBook)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            connection.Open();

            string query =
                @"UPDATE Books
                  SET Title = @Title,
                      Author = @Author
                  WHERE Id = @Id";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Title", updatedBook.Title);
            command.Parameters.AddWithValue("@Author", updatedBook.Author);
            command.Parameters.AddWithValue("@Id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            connection.Open();

            string query =
                "DELETE FROM Books WHERE Id = @Id";

            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }
}