using LibraryApi.Data;
using LibraryApi.DTOs.Admin;
using LibraryApi.Models.Status;
using LibraryApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Services;

public class AdminService : IAdminService
{
    private readonly LibraryDbContext _context;

    public AdminService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatisticsResponse> GetStatisticsAsync()
    {
        AdminStatisticsResponse response = new AdminStatisticsResponse
        {
            TotalBooks = await _context.Books.CountAsync(),

            AvailableBooks = await _context.Books
                .CountAsync(b => b.Status == BookStatus.Available),

            LoanedBooks = await _context.Books
                .CountAsync(b => b.Status == BookStatus.Loaned),

            TotalMembers = await _context.Members.CountAsync(),

            ActiveLoans = await _context.Loans
                .CountAsync(l => l.Status == LoanStatus.Active),

            OverdueLoans = await _context.Loans
                .CountAsync(l => l.Status == LoanStatus.Overdue),

            PendingLoanRequests = await _context.LoanRequest
                .CountAsync(r => r.Status == LoanRequestStatus.Pending),

            PendingExtensionRequests = await _context.LoanExtensionRequest
                .CountAsync(r => r.Status == LoanExtensionStatus.Pending)
        };

        return response;
    }
}