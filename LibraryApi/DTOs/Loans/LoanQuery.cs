using LibraryApi.Models;
using LibraryApi.Models.Status;

namespace LibraryApi.DTOs.Loans;

public class LoanQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public LoanStatus? Status { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}