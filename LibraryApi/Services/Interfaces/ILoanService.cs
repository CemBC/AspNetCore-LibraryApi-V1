using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;

namespace LibraryApi.Services.Interfaces;

public interface ILoanService
{
    Task<PagedResponse<LoanResponse>> GetAllAsync(LoanQuery query);

    Task<Loan> GetById(int id);

    Task<Loan> CreateLoan( CreateLoanRequest request, int userId);

    Task<Loan> CreateFromRequestAsync(LoanRequest request, int userId);

    Task ReturnBook( int loanId, int userId);

    Task UpdateOverdueLoansAsync();

    Task<List<LoanResponse>> GetMyLoansAsync( int userId);

    Task<PagedResponse<LoanResponse>> GetActiveLoansAsync( LoanQuery query);

    Task<PagedResponse<LoanResponse>> GetOverdueLoansAsync( LoanQuery query);
}