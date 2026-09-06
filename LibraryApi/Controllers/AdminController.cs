using LibraryApi.DTOs.Admin;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services.Interfaces;
namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<AdminStatisticsResponse>> GetStatistics()
    {
        AdminStatisticsResponse response = await _adminService.GetStatisticsAsync();

        return Ok(response);
    }
}