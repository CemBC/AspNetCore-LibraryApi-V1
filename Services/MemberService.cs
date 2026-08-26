using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class MemberService
{
    private readonly LibraryDbContext _context;

    private readonly IMapper _mapper;

    public MemberService(LibraryDbContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<PagedResponse<MemberResponse>> GetAllAsync(MemberQuery query)
    {
        IQueryable<Member> membersQuery = _context.Members.Include(m => m.User).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            membersQuery = membersQuery.Where(m => m.FullName.Contains(search) ||m.User.Email.Contains(search));
        }

        membersQuery = query.SortBy?.ToLower() switch
        {
            "fullname" => query.Descending
                ? membersQuery.OrderByDescending(m => m.FullName)
                : membersQuery.OrderBy(m => m.FullName),

            "email" => query.Descending
                ? membersQuery.OrderByDescending(m => m.User.Email)
                : membersQuery.OrderBy(m => m.User.Email),

            _ => membersQuery.OrderBy(m => m.Id)
        };

        int totalCount = await membersQuery.CountAsync();

        List<Member> members = await membersQuery.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

        return new PagedResponse<MemberResponse>
        {
            Items = _mapper.Map<List<MemberResponse>>(members),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }


    public async Task<Member> GetByIdAsync(int id)
    {
        Member? member = await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);


        if (member is null)
            throw new NotFoundException("Member not found.");


        return member;
    }


    public async Task<Member> CreateAsync(CreateMemberRequest request)
    {
        Member member = new Member
        {
            FullName = request.FullName
        };


        _context.Members.Add(member);

        await _context.SaveChangesAsync();


        return member;
    }


    public async Task UpdateAsync(
        int id,
        UpdateMemberRequest request)
    {
        Member? member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);


        if (member is null)
            throw new NotFoundException("Member not found.");


        member.FullName = request.FullName;


        await _context.SaveChangesAsync();
    }


    public async Task DeleteAsync(int id)
    {
        Member? member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);


        if (member is null)
            throw new NotFoundException("Member not found.");


        _context.Members.Remove(member);

        await _context.SaveChangesAsync();
    }

    public async Task<List<LoanResponse>> GetMemberLoansAsync(int memberId)
    {
        bool memberExists =  await _context.Members.AnyAsync(m => m.Id == memberId);

        if (!memberExists) throw new NotFoundException("Member not found.");


        List<Loan> loans =
            await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.MemberId == memberId)
                .OrderByDescending(l => l.LoanDate)
                .AsNoTracking()
                .ToListAsync();


        return _mapper.Map<List<LoanResponse>>(loans);
    }
}