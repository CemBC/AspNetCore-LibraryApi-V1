using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Common;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace LibraryApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);


    private readonly LoanService _loanService;
    private readonly IValidator<CreateLoanRequest> _loanValidator;
    private readonly IMapper _mapper;
    private readonly IValidator<LoanQuery> _loanQueryValidator;


    public LoansController(
        LoanService loanService,
        IValidator<CreateLoanRequest> loanValidator,
        IMapper mapper,
        IValidator<LoanQuery> loanQueryValidator)
    {
        _loanService = loanService;
        _loanValidator = loanValidator;
        _mapper = mapper;
        _loanQueryValidator = loanQueryValidator;
    }


    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetAll([FromQuery] LoanQuery query)
    {
        var validationResult = await _loanQueryValidator.ValidateAsync(query);
        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);


        var response = await _loanService.GetAllAsync(query);

        return Ok(response);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponse>> GetById(int id)
    {
        Loan loan = await _loanService.GetById(id);

        LoanResponse response =
            _mapper.Map<LoanResponse>(loan);

        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<LoanResponse>> CreateLoan(
        CreateLoanRequest request)
    {
        var validationResult = await _loanValidator.ValidateAsync(request);


        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);


        Loan createdLoan =await _loanService.CreateLoan(request , CurrentUserId);

        LoanResponse response = _mapper.Map<LoanResponse>(createdLoan);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/return")]
    public async Task<ActionResult> ReturnBook(int id)
    {
        await _loanService.ReturnBook(id , CurrentUserId);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("update-overdue")]
    public async Task<IActionResult> UpdateOverdue()
    {
        await _loanService.UpdateOverdueLoansAsync();

        return Ok();
    }



    [Authorize(Roles = "Member")]
    [HttpGet("my")]
    public async Task<ActionResult<List<LoanResponse>>> GetMyLoans()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        if (userId is null) return Unauthorized();
       

        List<LoanResponse> response = await _loanService.GetMyLoansAsync(int.Parse(userId));


        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("active")]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetActiveLoans([FromQuery] LoanQuery query)
    {
        var validationResult = await _loanQueryValidator.ValidateAsync(query);

        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var response = await _loanService.GetActiveLoansAsync(query);

        return Ok(response);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("overdue")]
    public async Task<ActionResult<PagedResponse<LoanResponse>>> GetOverdueLoans([FromQuery] LoanQuery query)
    {
        var validationResult = await _loanQueryValidator.ValidateAsync(query);

        if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

        var response = await _loanService.GetOverdueLoansAsync(query);

        return Ok(response);
    }

}