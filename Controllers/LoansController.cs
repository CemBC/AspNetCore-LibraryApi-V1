using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace LibraryApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly LoanService _loanService;
    private readonly IValidator<CreateLoanRequest> _loanValidator;
    private readonly IMapper _mapper;


    public LoansController(
        LoanService loanService,
        IValidator<CreateLoanRequest> loanValidator,
        IMapper mapper)
    {
        _loanService = loanService;
        _loanValidator = loanValidator;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<List<LoanResponse>>> GetAll()
    {
        List<Loan> loans = await _loanService.GetAll();

        List<LoanResponse> response =
            _mapper.Map<List<LoanResponse>>(loans);

        return Ok(response);
    }


    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanResponse>> GetById(int id)
    {
        Loan loan = await _loanService.GetById(id);

        LoanResponse response =
            _mapper.Map<LoanResponse>(loan);

        return Ok(response);
    }


    [HttpPost]
    public async Task<ActionResult<LoanResponse>> CreateLoan(
        CreateLoanRequest request)
    {
        var validationResult =
            await _loanValidator.ValidateAsync(request);


        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);


        Loan createdLoan =
            await _loanService.CreateLoan(request);


        LoanResponse response =
            _mapper.Map<LoanResponse>(createdLoan);


        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }


    [HttpPut("{id:int}/return")]
    public async Task<ActionResult> ReturnBook(int id)
    {
        await _loanService.ReturnBook(id);

        return NoContent();
    }
}