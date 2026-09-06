using AutoMapper;
using LibraryApi.Data;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Exceptions;
using LibraryApi.Models;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace LibraryApi.Services;

public class MemberService :IMemberService
{
    private readonly LibraryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<MemberService> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailService _emailService;

    public MemberService(
        LibraryDbContext context,
        IMapper mapper,
        ILogger<MemberService> logger,
        IPasswordHasher<User> passwordHasher,
        IEmailService emailService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<PagedResponse<MemberResponse>> GetAllAsync(MemberQuery query)
    {
        IQueryable<Member> membersQuery = _context.Members
            .Include(m => m.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search = query.Search.Trim();

            membersQuery = membersQuery.Where(m =>
                m.FullName.Contains(search) ||
                m.User.Email.Contains(search));
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

        List<Member> members = await membersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

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
            .Include(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException("Member not found.");
        }

        return member;
    }

    public async Task<Member> CreateAsync(CreateMemberRequest request, int userId)
    {
        Member member = new Member
        {
            FullName = request.FullName
        };

        _context.Members.Add(member);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Member with ID: {MemberId} created by the User with ID: {UserId}",
            member.Id,
            userId);

        return member;
    }

    public async Task UpdateAsync(int id, UpdateMemberRequest request, int userId)
    {
        Member? member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException("Member not found.");
        }

        member.FullName = request.FullName;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Member with ID: {MemberId} has been updated by the User with ID: {UserId}",
            member.Id,
            userId);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        Member? member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            throw new NotFoundException("Member not found.");
        }

        bool hasActiveLoan = await _context.Loans
            .AnyAsync(l =>
                l.MemberId == id &&
                (l.Status == LoanStatus.Active ||
                 l.Status == LoanStatus.Overdue));

        if (hasActiveLoan)
        {
            throw new BadRequestException(
                "Member cannot be deleted while they have an active loan.");
        }

        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Member with ID: {MemberId} has been deleted by the User with ID: {UserId}",
            member.Id,
            userId);
    }

    public async Task<List<LoanResponse>> GetMemberLoansAsync(int memberId)
    {
        bool memberExists = await _context.Members
            .AnyAsync(m => m.Id == memberId);

        if (!memberExists)
        {
            throw new NotFoundException("Member not found.");
        }

        List<Loan> loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.LoanDate)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<LoanResponse>>(loans);
    }

    public async Task ResetPasswordAsync(int memberId, int adminUserId)
    {
        Member? member = await _context.Members
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member is null)
        {
            throw new NotFoundException("Member not found.");
        }

        if (member.User is null)
        {
            throw new BadRequestException("This member does not have a user account.");
        }

        string temporaryPassword = CreateTemporaryPassword();

        string html = $"""
            <h2>Library Password Reset</h2>

            <p>An administrator has reset your Library account password.</p>

            <p>Your temporary password is:</p>

            <h2>{temporaryPassword}</h2>

            <p>Please sign in using this temporary password and change your password from your profile.</p>

            <p>If you did not expect this password reset, please contact the library administrator.</p>
            """;

        await _emailService.SendAsync(
            member.User.Email,
            "Your Library temporary password",
            html);

        member.User.PasswordHash = _passwordHasher.HashPassword(
            member.User,
            temporaryPassword);

        member.User.RefreshTokenHash = null;
        member.User.RefreshTokenExpiryTime = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Password for member {MemberId} was reset by admin user {AdminUserId}.",
            member.Id,
            adminUserId);
    }

    private static string CreateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@$%";
        const string all = upper + lower + digits + special;

        char[] password = new char[12];

        password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        for (int i = password.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}