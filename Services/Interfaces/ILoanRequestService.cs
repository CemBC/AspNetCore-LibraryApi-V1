using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanRequests;

namespace LibraryApi.Services.Interfaces
{
    public interface ILoanRequestService
    {
        Task<LoanRequestResponse> CreateAsync(int userId, CreateLoanRequestDto request);

        Task<PagedResponse<LoanRequestResponse>> GetPendingRequestsAsync(LoanRequestQuery query);

        Task<LoanRequestResponse> ApproveAsync(int id , int UserId);

        Task<LoanRequestResponse> RejectAsync(int id , int UserId);

        Task<List<LoanRequestResponse>> GetMyPendingRequestsAsync(int userId);

    }


}
