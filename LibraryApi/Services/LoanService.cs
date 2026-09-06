using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.Exceptions;
using LibraryApi.Helpers;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;

    private readonly IMapper _mapper;

    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext context, IMapper mapper , ILogger<LoanService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PagedResponse<LoanResponse>> GetAllAsync(LoanQuery query)
    {
        IQueryable<Loan> loansQuery = _context.Loans.Include(l => l.Book).Include(l => l.Member).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            loansQuery = loansQuery.Where(l =>l.Book.Title.Contains(search) ||l.Member.FullName.Contains(search));
        }

        if (query.Status.HasValue)
        {
            loansQuery = loansQuery.Where(l =>l.Status == query.Status.Value);
        }

        loansQuery = query.SortBy?.ToLower() switch
        {
            "loandate" => query.Descending ? loansQuery.OrderByDescending(l => l.LoanDate) : loansQuery.OrderBy(l => l.LoanDate),

            "duedate" => query.Descending ? loansQuery.OrderByDescending(l => l.DueDate) : loansQuery.OrderBy(l => l.DueDate),

            "booktitle" => query.Descending ? loansQuery.OrderByDescending(l => l.Book.Title) : loansQuery.OrderBy(l => l.Book.Title),

            "membername" => query.Descending ? loansQuery.OrderByDescending(l => l.Member.FullName) : loansQuery.OrderBy(l => l.Member.FullName),

            _ => loansQuery.OrderBy(l => l.Id)
        };

        int totalCount = await loansQuery.CountAsync();

        List<Loan> loans = await loansQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<LoanResponse>
        {
            Items = _mapper.Map<List<LoanResponse>>(loans),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }


    public async Task<Loan> GetById(int id)
    {
        Loan? loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);


        if (loan is null)
            throw new NotFoundException("Loan not found.");


        return loan;
    }


    public async Task<Loan> CreateLoan(CreateLoanRequest request , int UserId)
    {
        Book? book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.BookId);


        if (book is null)
            throw new NotFoundException("Book not found.");


        bool memberExists = await _context.Members
            .AnyAsync(m => m.Id == request.MemberId);


        if (!memberExists)
            throw new NotFoundException("Member not found.");


        if (book.Status != BookStatus.Available)
            throw new BadRequestException("Book is not available for loan.");


        bool hasOverdueLoan = await _context.Loans.AnyAsync(l => l.MemberId == request.MemberId && l.ReturnDate == null && l.DueDate < DateTime.UtcNow);

        if (hasOverdueLoan)throw new BadRequestException("This member has an overdue loan and cannot receive another book.");
        

        int currentLoanCount = await _context.Loans.CountAsync(l => l.MemberId == request.MemberId && l.ReturnDate == null);

        if (currentLoanCount >= LibraryRules.MaxActiveLoansPerMember) throw new BadRequestException( $"This member already has the maximum of {LibraryRules.MaxActiveLoansPerMember} active loans.");
        

        Loan loan = new Loan
        {
            BookId = request.BookId,
            MemberId = request.MemberId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            ReturnDate = null,
            Status = LoanStatus.Active
        };


        await _context.Loans.AddAsync(loan);

        book.Status = BookStatus.Loaned;


        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan with ID: {LoanId} has been created by the User with ID: {UserId}" , loan.Id , UserId);

        return loan;
    }

    public async Task<Loan> CreateFromRequestAsync(LoanRequest request , int UserId)
    {
        Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == request.BookId);


        if (book is null) throw new NotFoundException("Book not found.");



        Loan loan = new Loan
        {
            BookId = request.BookId,

            MemberId = request.MemberId,

            LoanDate = DateTime.UtcNow,

            DueDate = DateTime.UtcNow.AddDays(7),

            ReturnDate = null,

            Status = LoanStatus.Active
        };


        book.Status = BookStatus.Loaned;


        await _context.Loans.AddAsync(loan);


        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan wih ID: {LoanId} of the Member with ID: {MemberId} has been created by the User with ID: {UserId}" , loan.Id , loan.MemberId , UserId);

        return loan;
    }


    public async Task ReturnBook(int loanId , int UserId)
    {
        Loan? loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan is null) throw new NotFoundException("Loan not found.");

        if (loan.Status == LoanStatus.Returned)throw new BadRequestException("Loan has already been returned.");
        
        Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == loan.BookId);
        if (book is null) throw new NotFoundException("Book not found.");
        
        loan.ReturnDate = DateTime.UtcNow;
        loan.Status = LoanStatus.Returned;

        book.Status = BookStatus.Available;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Loan with ID: {LoanId} returned by Member with ID: {MemberId} with the acceptance of User with ID: {UserId} . Book  with ID: {BookId} is available again.",
            loan.Id,
            loan.MemberId,
            UserId,
            loan.BookId);
    }

    public async Task UpdateOverdueLoansAsync()
    {
        List<Loan> overdueLoans =
            await _context.Loans
            .Where(l =>
                l.Status == LoanStatus.Active &&
                l.DueDate < DateTime.UtcNow)
            .ToListAsync();


        foreach (Loan loan in overdueLoans)
        {
            loan.Status = LoanStatus.Overdue;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("{Count} loans marked as overdue.",
            overdueLoans.Count);

    }


    public async Task<List<LoanResponse>> GetMyLoansAsync(int userId)
    {
        Member? member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);


        if (member is null) throw new NotFoundException("Member not found.");


        List<Loan> loans =
            await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == member.Id)
            .AsNoTracking()
            .ToListAsync();


        return _mapper.Map<List<LoanResponse>>(loans);
    }

    public async Task<PagedResponse<LoanResponse>> GetActiveLoansAsync(LoanQuery query)
    {
        IQueryable<Loan> loansQuery = _context.Loans.Include(l => l.Book).Include(l => l.Member)
            .Where(l => l.Status == LoanStatus.Active)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            loansQuery = loansQuery.Where(l =>l.Book.Title.Contains(search) || l.Member.FullName.Contains(search));
        }

        loansQuery = query.SortBy?.ToLower() switch
        {
            "loandate" => query.Descending ? loansQuery.OrderByDescending(l => l.LoanDate) : loansQuery.OrderBy(l => l.LoanDate),

            "duedate" => query.Descending ? loansQuery.OrderByDescending(l => l.DueDate) : loansQuery.OrderBy(l => l.DueDate),

            "booktitle" => query.Descending ? loansQuery.OrderByDescending(l => l.Book.Title) : loansQuery.OrderBy(l => l.Book.Title),

            "membername" => query.Descending ? loansQuery.OrderByDescending(l => l.Member.FullName) : loansQuery.OrderBy(l => l.Member.FullName),

            _ => loansQuery.OrderBy(l => l.DueDate)
        };

        int totalCount = await loansQuery.CountAsync();

        List<Loan> loans = await loansQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<LoanResponse>
        {
            Items = _mapper.Map<List<LoanResponse>>(loans),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    public async Task<PagedResponse<LoanResponse>> GetOverdueLoansAsync(LoanQuery query)
    {
        IQueryable<Loan> loansQuery = _context.Loans.Include(l => l.Book).Include(l => l.Member)
            .Where(l => l.Status == LoanStatus.Overdue)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            loansQuery = loansQuery.Where(l =>l.Book.Title.Contains(search) || l.Member.FullName.Contains(search));
        }

        loansQuery = query.SortBy?.ToLower() switch
        {
            "loandate" => query.Descending ? loansQuery.OrderByDescending(l => l.LoanDate) : loansQuery.OrderBy(l => l.LoanDate),

            "duedate" => query.Descending ? loansQuery.OrderByDescending(l => l.DueDate) : loansQuery.OrderBy(l => l.DueDate),

            "booktitle" => query.Descending ? loansQuery.OrderByDescending(l => l.Book.Title) : loansQuery.OrderBy(l => l.Book.Title),

            "membername" => query.Descending ? loansQuery.OrderByDescending(l => l.Member.FullName) : loansQuery.OrderBy(l => l.Member.FullName),

            _ => loansQuery.OrderBy(l => l.DueDate)
        };

        int totalCount = await loansQuery.CountAsync();

        List<Loan> loans = await loansQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<LoanResponse>
        {
            Items = _mapper.Map<List<LoanResponse>>(loans),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }
}