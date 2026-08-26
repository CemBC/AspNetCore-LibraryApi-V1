using FluentValidation;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.LoanRequests;
using LibraryApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanRequestsController : ControllerBase
{
    private readonly ILoanRequestService _loanRequestService;

    private readonly IValidator<LoanRequestQuery> _loanRequestQueryValidator;
    public LoanRequestsController(
        ILoanRequestService loanRequestService , IValidator<LoanRequestQuery> loanRequestQueryValidator)
    {
        _loanRequestService = loanRequestService;
        _loanRequestQueryValidator = loanRequestQueryValidator;
    }


    [HttpGet("pending/my")]
    public async Task<ActionResult<LoanRequestResponse>> GetMyPending()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if(userId == null) return Unauthorized();

        List<LoanRequestResponse> request = await _loanRequestService.GetMyPendingRequestsAsync(int.Parse(userId)); 

        return Ok(request);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/approve")]
    public async Task<ActionResult<LoanRequestResponse>> Approve(int id)
    {
        LoanRequestResponse response = await _loanRequestService.ApproveAsync(id);


        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/reject")]
    public async Task<ActionResult<LoanRequestResponse>> Reject(int id)
    {
        LoanRequestResponse response = await _loanRequestService.RejectAsync(id);


        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<PagedResponse<LoanRequestResponse>>> GetPending(
    [FromQuery] LoanRequestQuery query)
    {
        var validationResult = await _loanRequestQueryValidator.ValidateAsync(query);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var response = await _loanRequestService.GetPendingRequestsAsync(query);

        return Ok(response);
    }


    [Authorize(Roles = "Member")]
    [HttpPost]
    public async Task<ActionResult<LoanRequestResponse>> Create(
        CreateLoanRequestDto request)
    {
        string? userId =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        if (userId is null)
            return Unauthorized();


        LoanRequestResponse response =
            await _loanRequestService.CreateAsync(
                int.Parse(userId),
                request);


        return Ok(response);
    }
}