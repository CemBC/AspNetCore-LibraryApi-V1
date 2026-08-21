using LibraryApi.Data;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class MemberService
{
    private readonly LibraryDbContext _context;

    public MemberService(LibraryDbContext context)
    {
        _context = context;
    }


    public async Task<List<Member>> GetAllAsync()
    {
        return await _context.Members
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }


    public async Task<Member> CreateAsync(CreateMemberRequest request)
    {
        var member = new Member
        {
            FullName = request.FullName,
            Email = request.Email
        };


        _context.Members.Add(member);

        await _context.SaveChangesAsync();

        return member;
    }


    public async Task<bool> UpdateAsync(
        int id,
        UpdateMemberRequest request)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);


        if (member == null)
            return false;


        member.FullName = request.FullName;
        member.Email = request.Email;


        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteAsync(int id)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id);


        if (member == null)
            return false;


        _context.Members.Remove(member);

        await _context.SaveChangesAsync();

        return true;
    }
}