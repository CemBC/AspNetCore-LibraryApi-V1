using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanExtensions;

namespace LibraryApi.Services;

public interface ILoanExtensionService
{
    Task<LoanExtensionResponse> CreateAsync(int userId,CreateLoanExtensionRequestDto request);

    Task<PagedResponse<LoanExtensionResponse>> GetPendingRequestsAsync(LoanExtensionRequestQuery query);

    Task<LoanExtensionResponse> ApproveAsync(int id);

    Task<LoanExtensionResponse> RejectAsync(int id);

    Task<List<LoanExtensionResponse>> GetMyPendingRequestsAsync(int UserId);

}