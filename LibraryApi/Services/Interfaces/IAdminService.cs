using LibraryApi.DTOs.Admin;

namespace LibraryApi.Services.Interfaces;

public interface IAdminService
{
    Task<AdminStatisticsResponse> GetStatisticsAsync();
}