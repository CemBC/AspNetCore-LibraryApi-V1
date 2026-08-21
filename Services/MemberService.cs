using LibraryApi.Data;
using LibraryApi.DTOs.Members;
using LibraryApi.Exceptions;
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
            FullName = request.FullName,
            Email = request.Email
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
        member.Email = request.Email;


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
}