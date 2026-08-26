using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class LoanRequestService : ILoanRequestService
{
    private readonly LibraryDbContext _context;

    private readonly LoanService _loanService;
    private readonly IMapper _mapper;

    public LoanRequestService(LibraryDbContext context ,IMapper mapper , LoanService loanService)
    {
        _context = context;
        _mapper = mapper;   
        _loanService = loanService;
    }

    public async Task<LoanRequestResponse> ApproveAsync(int id)
    {
       LoanRequest? request = await _context.LoanRequest.Include(l => l.Book).Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);

        if(request is null) throw new NotFoundException("Loan request not found."); 
        if(request.Status != LoanRequestStatus.Pending) throw new BadRequestException("Only pending requests can be approved.");

        await _loanService.CreateFromRequestAsync(request);

        request.Status = LoanRequestStatus.Approved;

        await _context.SaveChangesAsync();

        return _mapper.Map<LoanRequestResponse>(request);
    }

    public async Task<LoanRequestResponse> CreateAsync(
        int userId,
        CreateLoanRequestDto request)
    {
        
        Member? member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

        if (member is null) throw new NotFoundException("Member not found.");
        



        Book? book = await _context.Books.FirstOrDefaultAsync(b => b.Id == request.BookId);

        if (book is null) throw new NotFoundException("Book not found.");
        

        if (book.Status != BookStatus.Available) throw new BadRequestException("Book is not available.");

        bool hasPendingRequest =  await _context.LoanRequest.AnyAsync(l =>l.BookId == request.BookId &&l.Status == LoanRequestStatus.Pending);

        if (hasPendingRequest) throw new BadRequestException("A pending request already exists for this book.");

        LoanRequest loanRequest = new LoanRequest
        {
            BookId = book.Id,

            MemberId = member.Id,

            Book = book,
            Member = member,

            LoanCode = GenerateLoanCode(),

            RequestDate = DateTime.UtcNow,

            Status = LoanRequestStatus.Pending
        };


        book.Status = BookStatus.Requested;

        await _context.LoanRequest.AddAsync(loanRequest);

        await _context.SaveChangesAsync();

        return _mapper.Map<LoanRequestResponse>(loanRequest);
    }

    public async Task<List<LoanRequestResponse>> GetMyPendingRequestsAsync(int userId)
    {
        Member? member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
        if (member is null) throw new NotFoundException("Member not found.");

        List<LoanRequest> requests = await _context.LoanRequest.Include(l => l.Book)
            .Where(l => l.MemberId == member.Id && l.Status == LoanRequestStatus.Pending)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<LoanRequestResponse>>(requests);
    }

    public async Task<PagedResponse<LoanRequestResponse>> GetPendingRequestsAsync(LoanRequestQuery query)
    {
        IQueryable<LoanRequest> requestsQuery = _context.LoanRequest.Include(r => r.Book).Include(r => r.Member).Where(r => r.Status == LoanRequestStatus.Pending)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            requestsQuery = requestsQuery.Where(r =>r.Book.Title.Contains(search) ||r.Member.FullName.Contains(search) ||r.LoanCode.Contains(search));
        }

        requestsQuery = query.SortBy?.ToLower() switch
        {
            "requestdate" => query.Descending ? requestsQuery.OrderByDescending(r => r.RequestDate) : requestsQuery.OrderBy(r => r.RequestDate),

            "booktitle" => query.Descending ? requestsQuery.OrderByDescending(r => r.Book.Title) : requestsQuery.OrderBy(r => r.Book.Title),

            "membername" => query.Descending ? requestsQuery.OrderByDescending(r => r.Member.FullName) : requestsQuery.OrderBy(r => r.Member.FullName),

            _ => requestsQuery.OrderBy(r => r.RequestDate)
        };

        int totalCount = await requestsQuery.CountAsync();

        List<LoanRequest> requests = await requestsQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<LoanRequestResponse>
        {
            Items = _mapper.Map<List<LoanRequestResponse>>(requests),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }

    public async Task<LoanRequestResponse> RejectAsync(int id)
    {
        LoanRequest? request = await _context.LoanRequest.Include(l => l.Book).Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (request is null) throw new NotFoundException("Loan request not found.");
        if (request.Status != LoanRequestStatus.Pending) throw new BadRequestException("Only pending requests can be rejected.");

        request.Status = LoanRequestStatus.Rejected;

        request.Book.Status = BookStatus.Available;

        await _context.SaveChangesAsync();

        return _mapper.Map<LoanRequestResponse>(request);

    }

    private string GenerateLoanCode()
    {
        return $"LR-{Guid.NewGuid()
            .ToString("N")
            .Substring(0, 6)
            .ToUpper()}";
    }
}