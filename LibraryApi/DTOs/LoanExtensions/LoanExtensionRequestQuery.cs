namespace LibraryApi.DTOs.LoanExtensions;

public class LoanExtensionRequestQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public bool Descending { get; set; }
}