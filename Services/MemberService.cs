using LibraryApi.Data;
using LibraryApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services
{
    public class MemberService
    {
        private readonly LibraryDbContext _context;

        public MemberService(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<List<Member>> GetAll()
        {
            return await _context.Members.AsNoTracking().ToListAsync();
        }

        public async Task<Member?> GetById(int id)
        {
            return await _context.Members.AsNoTracking().
                FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddMember(Member member)
        {
             await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Update(int id, Member updatedMember)
        {
            Member? member = await _context.Members.FindAsync(id);
            if (member == null) return false;

            member.FullName = updatedMember.FullName;
            member.Email = updatedMember.Email;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            Member? member = await _context.Members.FindAsync(id);
            if (member == null) return false;

            _context.Members.Remove(member);

            await _context.SaveChangesAsync();

            return true;
        }
    }
}