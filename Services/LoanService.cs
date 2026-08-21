using LibraryApi.Data;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services
{
    public class LoanService
    {
        private readonly LibraryDbContext _context;

        public LoanService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Loan>> GetAll()
        {
            return await _context.Loans
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Loan?> GetById(int id)
        {
            return await _context.Loans
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<(LoanOperationStatus Status, Loan? CreatedLoan)> CreateLoan(
            CreateLoanRequest request)
        {
            Book? book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == request.BookId);

            if (book is null)
                return (LoanOperationStatus.BookNotFound, null);


            bool memberExists = await _context.Members
                .AnyAsync(m => m.Id == request.MemberId);

            if (!memberExists)
                return (LoanOperationStatus.MemberNotFound, null);


            if (!book.IsAvailable)
                return (LoanOperationStatus.BookUnavailable, null);


            Loan loan = new Loan
            {
                BookId = request.BookId,
                MemberId = request.MemberId,
                LoanDate = DateTime.Now,
                ReturnDate = null
            };

            await _context.Loans.AddAsync(loan);

            book.IsAvailable = false;

            await _context.SaveChangesAsync();

            return (LoanOperationStatus.Success, loan);
        }

        public async Task<LoanOperationStatus> ReturnBook(int loanId)
        {
            Loan? loan = await _context.Loans
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan is null)
                return LoanOperationStatus.LoanNotFound;


            if (loan.ReturnDate is not null)
                return LoanOperationStatus.AlreadyReturned;


            Book? book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == loan.BookId);

            if (book is null)
                return LoanOperationStatus.BookNotFound;


            loan.ReturnDate = DateTime.Now;

            book.IsAvailable = true;

            await _context.SaveChangesAsync();

            return LoanOperationStatus.Success;
        }
    }
}