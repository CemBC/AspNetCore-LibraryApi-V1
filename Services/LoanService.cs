using LibraryApi.Models;
using Microsoft.Data.SqlClient;

namespace LibraryApi.Services
{
    public class LoanService
    {
        private readonly string _connectionString;

        public LoanService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("LibraryDb")!;
        }

        public List<Loan> GetAll()
        {
            List<Loan> loans = new();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                @"SELECT Id, BookId, MemberId, LoanDate, ReturnDate
                  FROM Loans";

            using SqlCommand command =
                new SqlCommand(query, connection);

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Loan loan = new Loan
                {
                    Id = reader.GetInt32(0),
                    BookId = reader.GetInt32(1),
                    MemberId = reader.GetInt32(2),
                    LoanDate = reader.GetDateTime(3),
                    ReturnDate = reader.IsDBNull(4)
                        ? null
                        : reader.GetDateTime(4)
                };

                loans.Add(loan);
            }

            return loans;
        }

        public Loan? GetById(int id)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                @"SELECT Id, BookId, MemberId, LoanDate, ReturnDate
                  FROM Loans
                  WHERE Id = @Id";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader =
                command.ExecuteReader();

            if (reader.Read())
            {
                return new Loan
                {
                    Id = reader.GetInt32(0),
                    BookId = reader.GetInt32(1),
                    MemberId = reader.GetInt32(2),
                    LoanDate = reader.GetDateTime(3),
                    ReturnDate = reader.IsDBNull(4)
                        ? null
                        : reader.GetDateTime(4)
                };
            }

            return null;
        }

        public LoanOperationStatus CreateLoan(
            int bookId,
            int memberId,
            out Loan? createdLoan)
        {
            createdLoan = null;

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                string bookQuery =
                    @"SELECT IsAvailable
                      FROM Books
                      WHERE Id = @BookId";

                using SqlCommand bookCommand =
                    new SqlCommand(bookQuery, connection, transaction);

                bookCommand.Parameters.AddWithValue("@BookId", bookId);

                object? bookResult = bookCommand.ExecuteScalar();

                if (bookResult is null)
                {
                    transaction.Rollback();
                    return LoanOperationStatus.BookNotFound;
                }

                bool isAvailable = Convert.ToBoolean(bookResult);

                if (!isAvailable)
                {
                    transaction.Rollback();
                    return LoanOperationStatus.BookUnavailable;
                }


                string memberQuery =
                    @"SELECT Id
                      FROM Members
                      WHERE Id = @MemberId";

                using SqlCommand memberCommand =
                    new SqlCommand(memberQuery, connection, transaction);

                memberCommand.Parameters.AddWithValue("@MemberId", memberId);

                object? memberResult = memberCommand.ExecuteScalar();

                if (memberResult is null)
                {
                    transaction.Rollback();
                    return LoanOperationStatus.MemberNotFound;
                }


                DateTime loanDate = DateTime.Now;

                string insertQuery =
                    @"INSERT INTO Loans (BookId, MemberId, LoanDate, ReturnDate)
                      OUTPUT INSERTED.Id
                      VALUES (@BookId, @MemberId, @LoanDate, NULL)";

                using SqlCommand insertCommand =
                    new SqlCommand(insertQuery, connection, transaction);

                insertCommand.Parameters.AddWithValue("@BookId", bookId);
                insertCommand.Parameters.AddWithValue("@MemberId", memberId);
                insertCommand.Parameters.AddWithValue("@LoanDate", loanDate);

                int loanId =
                    Convert.ToInt32(insertCommand.ExecuteScalar());


                string updateBookQuery =
                    @"UPDATE Books
                      SET IsAvailable = 0
                      WHERE Id = @BookId";

                using SqlCommand updateBookCommand =
                    new SqlCommand(updateBookQuery, connection, transaction);

                updateBookCommand.Parameters.AddWithValue("@BookId", bookId);

                updateBookCommand.ExecuteNonQuery();


                transaction.Commit();

                createdLoan = new Loan
                {
                    Id = loanId,
                    BookId = bookId,
                    MemberId = memberId,
                    LoanDate = loanDate,
                    ReturnDate = null
                };

                return LoanOperationStatus.Success;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public LoanOperationStatus ReturnBook(int loanId)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            using SqlTransaction transaction =
                connection.BeginTransaction();

            try
            {
                string loanQuery =
                    @"SELECT BookId, ReturnDate
                      FROM Loans
                      WHERE Id = @LoanId";

                using SqlCommand loanCommand =
                    new SqlCommand(loanQuery, connection, transaction);

                loanCommand.Parameters.AddWithValue("@LoanId", loanId);

                using SqlDataReader reader =
                    loanCommand.ExecuteReader();

                if (!reader.Read())
                {
                    reader.Close();
                    transaction.Rollback();

                    return LoanOperationStatus.LoanNotFound;
                }

                int bookId = reader.GetInt32(0);

                bool alreadyReturned =
                    !reader.IsDBNull(1);

                reader.Close();

                if (alreadyReturned)
                {
                    transaction.Rollback();

                    return LoanOperationStatus.AlreadyReturned;
                }


                string updateLoanQuery =
                    @"UPDATE Loans
                      SET ReturnDate = @ReturnDate
                      WHERE Id = @LoanId";

                using SqlCommand updateLoanCommand =
                    new SqlCommand(updateLoanQuery, connection, transaction);

                updateLoanCommand.Parameters.AddWithValue(
                    "@ReturnDate",
                    DateTime.Now);

                updateLoanCommand.Parameters.AddWithValue(
                    "@LoanId",
                    loanId);

                updateLoanCommand.ExecuteNonQuery();


                string updateBookQuery =
                    @"UPDATE Books
                      SET IsAvailable = 1
                      WHERE Id = @BookId";

                using SqlCommand updateBookCommand =
                    new SqlCommand(updateBookQuery, connection, transaction);

                updateBookCommand.Parameters.AddWithValue(
                    "@BookId",
                    bookId);

                int affectedBooks =
                    updateBookCommand.ExecuteNonQuery();

                if (affectedBooks == 0)
                {
                    transaction.Rollback();

                    return LoanOperationStatus.BookNotFound;
                }


                transaction.Commit();

                return LoanOperationStatus.Success;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}