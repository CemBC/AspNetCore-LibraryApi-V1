using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using Microsoft.EntityFrameworkCore;
namespace LibraryApi.Services;

public class LoanExtensionService : ILoanExtensionService
{
    private readonly LibraryDbContext _context;
    private readonly IMapper _mapper;


    public LoanExtensionService(
        LibraryDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<LoanExtensionResponse> CreateAsync(int userId, CreateLoanExtensionRequestDto request)
    {
        Member? member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

        if (member is null) throw new NotFoundException("Member not found.");

        Loan? loan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == request.LoanId);
        if (loan is null) throw new NotFoundException("Loan not found.");
        
        if (loan.MemberId != member.Id) throw new BadRequestException("This loan does not belong to this member.");
        
        if (loan.Status != LoanStatus.Active)throw new BadRequestException("Only active loans can be extended.");
       
        bool extensionExists = await _context.LoanExtensionRequest.AnyAsync(x => x.LoanId == loan.Id && x.Status == LoanExtensionStatus.Pending);

        if (extensionExists)throw new BadRequestException( "An extension request already exists.");


        LoanExtensionRequest extensionRequest = new LoanExtensionRequest
        {
            LoanId = loan.Id,

            MemberId = member.Id,

            Loan = loan,

            Member = member,

            RequestDate = DateTime.UtcNow,

            Status = LoanExtensionStatus.Pending
        };



        await _context.LoanExtensionRequest
            .AddAsync(extensionRequest);



        await _context.SaveChangesAsync();



        return _mapper.Map<LoanExtensionResponse>(extensionRequest);
    }

    public async Task<PagedResponse<LoanExtensionResponse>> GetPendingRequestsAsync(LoanExtensionRequestQuery query)
    {
        IQueryable<LoanExtensionRequest> extensionsQuery = _context.LoanExtensionRequest.Include(e => e.Loan).ThenInclude(l => l.Book).Include(e => e.Member)
                .Where(e => e.Status == LoanExtensionStatus.Pending)
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            extensionsQuery = extensionsQuery.Where(e => e.Loan.Book.Title.Contains(search) || e.Member.FullName.Contains(search));
        }

        extensionsQuery = query.SortBy?.ToLower() switch
        {
            "requestdate" => query.Descending ? extensionsQuery.OrderByDescending(e => e.RequestDate): extensionsQuery.OrderBy(e => e.RequestDate),

            "booktitle" => query.Descending ? extensionsQuery.OrderByDescending(e => e.Loan.Book.Title) : extensionsQuery.OrderBy(e => e.Loan.Book.Title),

            "membername" => query.Descending ? extensionsQuery.OrderByDescending(e => e.Member.FullName) : extensionsQuery.OrderBy(e => e.Member.FullName),

            _ => extensionsQuery.OrderBy(e => e.RequestDate)
        };

        int totalCount = await extensionsQuery.CountAsync();

        List<LoanExtensionRequest> extensions = await extensionsQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<LoanExtensionResponse>
        {
            Items = _mapper.Map<List<LoanExtensionResponse>>(extensions),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }


    public async Task<LoanExtensionResponse> ApproveAsync(int id)
    {
        LoanExtensionRequest? request =await _context.LoanExtensionRequest.Include(x => x.Loan).ThenInclude(l => l.Book).Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request is null)throw new NotFoundException("Extension request not found.");        
        if (request.Status != LoanExtensionStatus.Pending) throw new BadRequestException("Extension request is not pending.");
        if (request.Loan.Status != LoanStatus.Active) throw new BadRequestException("Only active loans can be extended.");
        

        request.Status = LoanExtensionStatus.Approved;
        request.Loan.DueDate =  request.Loan.DueDate.AddDays(7);

        await _context.SaveChangesAsync();
        return _mapper.Map<LoanExtensionResponse>(request);
    }

    public async Task<LoanExtensionResponse> RejectAsync(int id)
    {
        LoanExtensionRequest? request = await _context.LoanExtensionRequest.Include(x => x.Loan).ThenInclude(l => l.Book).Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Id == id);


        if (request is null) throw new NotFoundException("Extension request not found.");
        if (request.Status != LoanExtensionStatus.Pending) throw new BadRequestException("Extension request is not pending.");
        

        request.Status = LoanExtensionStatus.Rejected;

        await _context.SaveChangesAsync();

        return _mapper.Map<LoanExtensionResponse>(request);
    }

    public async Task<List<LoanExtensionResponse>> GetMyPendingRequestsAsync(int userId)
    {
        Member? member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);


        if (member is null) throw new NotFoundException("Member not found.");


        List<LoanExtensionRequest> requests = await _context.LoanExtensionRequest.Include(e => e.Loan).ThenInclude(l => l.Book).Include(e => e.Member)
            .Where(e => e.MemberId == member.Id && e.Status == LoanExtensionStatus.Pending)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<LoanExtensionResponse>>(requests);
    }
}