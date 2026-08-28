using FluentValidation;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanExtensions;
using LibraryApi.Services;
using LibraryApi.Validators.LoanExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanExtensionsController : ControllerBase
{

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private readonly ILoanExtensionService _loanExtensionService;

    private readonly IValidator<LoanExtensionRequestQuery> _loanExtensionRequestQueryValidator;
    public LoanExtensionsController(
        ILoanExtensionService loanExtensionService , IValidator<LoanExtensionRequestQuery> loanExtensionRequestQueryValidator)
    {
        _loanExtensionService = loanExtensionService;
        _loanExtensionRequestQueryValidator = loanExtensionRequestQueryValidator;
    }



    [Authorize(Roles = "Member")]
    [HttpPost]
    public async Task<ActionResult<LoanExtensionResponse>> Create(
        CreateLoanExtensionRequestDto request)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null) return Unauthorized();

        LoanExtensionResponse response = await _loanExtensionService.CreateAsync(int.Parse(userId),request);

        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<PagedResponse<LoanExtensionResponse>>> GetPending(
    [FromQuery] LoanExtensionRequestQuery query)
    {
        var validationResult =  await _loanExtensionRequestQueryValidator.ValidateAsync(query);

        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var response = await _loanExtensionService.GetPendingRequestsAsync(query);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<LoanExtensionResponse>> Approve(int id)
    {
        LoanExtensionResponse response = await _loanExtensionService.ApproveAsync(id , CurrentUserId);

        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<LoanExtensionResponse>> Reject(int id)
    {
        LoanExtensionResponse response = await _loanExtensionService.RejectAsync(id , CurrentUserId);


        return Ok(response);
    }

    [Authorize(Roles = "Member")]
    [HttpGet("pending/my")]
    public async Task<ActionResult<List<LoanExtensionResponse>>> GetMyPendingExtensions()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null) return Unauthorized();
     
        var response = await _loanExtensionService.GetMyPendingRequestsAsync(int.Parse(userId));


        return Ok(response);
    }
}