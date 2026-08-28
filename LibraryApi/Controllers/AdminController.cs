using LibraryApi.DTOs.Admin;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
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