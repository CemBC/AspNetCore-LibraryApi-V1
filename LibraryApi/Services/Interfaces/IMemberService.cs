using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.DTOs.Members;
using LibraryApi.Models;

namespace LibraryApi.Services.Interfaces;

public interface IMemberService
{
    Task<PagedResponse<MemberResponse>> GetAllAsync( MemberQuery query);

    Task<Member> GetByIdAsync(int id);

    Task<Member> CreateAsync(CreateMemberRequest request, int userId);

    Task UpdateAsync( int id, UpdateMemberRequest request, int userId);

    Task DeleteAsync(int id,int userId);

    Task<List<LoanResponse>> GetMemberLoansAsync(int memberId);

    Task ResetPasswordAsync( int memberId, int adminUserId);
}