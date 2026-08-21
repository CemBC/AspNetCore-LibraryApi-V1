using LibraryApi.Data;
using LibraryApi.DTOs.Loans;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

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


    public async Task<Loan> GetById(int id)
    {
        Loan? loan = await _context.Loans
        .AsNoTracking()
        .FirstOrDefaultAsync(l => l.Id == id);


        if (loan is null)
            throw new NotFoundException("Loan not found.");


        return loan;
    }


    public async Task<Loan> CreateLoan(CreateLoanRequest request)
    {
        Book? book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId);


        if (book is null)
            throw new NotFoundException("Book not found.");


        bool memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId);


        if (!memberExists)
            throw new NotFoundException("Member not found.");


        if (!book.IsAvailable)
            throw new BadRequestException("Book is not available for loan.");


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


        return loan;
    }


    public async Task ReturnBook(int loanId)
    {
        Loan? loan = await _context.Loans
            .FirstOrDefaultAsync(l => l.Id == loanId);


        if (loan is null)
            throw new NotFoundException("Loan not found.");


        if (loan.ReturnDate is not null)
            throw new BadRequestException(
                "Loan has already been returned.");


        Book? book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == loan.BookId);


        if (book is null)
            throw new NotFoundException("Book not found.");


        loan.ReturnDate = DateTime.Now;

        book.IsAvailable = true;


        await _context.SaveChangesAsync();
    }
}