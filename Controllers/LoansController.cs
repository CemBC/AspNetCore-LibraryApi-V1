using AutoMapper;
using FluentValidation;
using LibraryApi.DTOs.Loans;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly LoanService _loanService;
        private readonly IValidator<CreateLoanRequest> _loanValidator;
        private readonly IMapper _mapper;
        public LoansController(LoanService loanService , IValidator<CreateLoanRequest> loanValidator, IMapper mapper)
        {
            _loanService = loanService;
            _loanValidator = loanValidator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<LoanResponse>>> GetAll()
        {
            List<Loan> loans = await _loanService.GetAll();

            List<LoanResponse> response = _mapper.Map<List<LoanResponse>>(loans);

            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LoanResponse>> GetById(int id)
        {
            Loan? loan = await _loanService.GetById(id);

            if (loan is null)
                return NotFound();

            LoanResponse response = _mapper.Map<LoanResponse>(loan);

            return response;
        }

        [HttpPost]
        public async Task<ActionResult<LoanResponse>> CreateLoan(
            CreateLoanRequest request)
        {
            var validationResult = await _loanValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            var (status, createdLoan) =  await _loanService.CreateLoan(request);

            if (status == LoanOperationStatus.BookNotFound)
                return NotFound("Book not found.");

            if (status == LoanOperationStatus.MemberNotFound)
                return NotFound("Member not found.");

            if (status == LoanOperationStatus.BookUnavailable)
                return BadRequest("This book is currently unavailable.");

            if (createdLoan is null)
                return StatusCode(500);

            LoanResponse response = _mapper.Map<LoanResponse>(createdLoan);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        [HttpPut("{id:int}/return")]
        public async Task<ActionResult> ReturnBook(int id)
        {
            LoanOperationStatus status =
                await _loanService.ReturnBook(id);

            if (status == LoanOperationStatus.LoanNotFound)
                return NotFound("Loan not found.");

            if (status == LoanOperationStatus.AlreadyReturned)
                return BadRequest("This book has already been returned.");

            if (status == LoanOperationStatus.BookNotFound)
                return NotFound("Book not found.");

            return NoContent();
        }
    }
}